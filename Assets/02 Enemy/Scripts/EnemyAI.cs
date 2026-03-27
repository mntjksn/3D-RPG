using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class EnemyAI : MonoBehaviour
{
    [Header("Data")]
    [SerializeField] private EnemyData enemyData;

    [Header("Target")]
    [SerializeField] private Transform target;

    private NavMeshAgent agent;
    private EnemyAnimation enemyAnimation;
    private EnemyAttack enemyAttack;

    private float patrolTimer;
    private float attackRecoverTimer;

    private Vector3 spawnPosition;

    private bool isPatrolling;
    private bool isChasing;
    private bool isReturning;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        enemyAnimation = GetComponent<EnemyAnimation>();
        enemyAttack = GetComponent<EnemyAttack>();
    }

    private void Start()
    {
        spawnPosition = transform.position;

        if (target == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
                target = player.transform;
        }

        ApplyData();

        enemyAnimation?.PlayIdle();

        enemyAttack?.SetData(enemyData);
        enemyAttack?.SetTarget(target);
    }

    private void Update()
    {
        if (enemyData == null || target == null)
            return;

        float distanceToTarget = Vector3.Distance(transform.position, target.position);
        float distanceToSpawn = Vector3.Distance(transform.position, spawnPosition);

        if (!isChasing && !isReturning && distanceToTarget <= enemyData.detectRange)
        {
            isChasing = true;
        }

        if (isChasing && distanceToTarget >= enemyData.loseRange)
        {
            isChasing = false;
            isReturning = true;
            agent.isStopped = false;
            agent.SetDestination(spawnPosition);
        }

        if (isReturning && distanceToTarget <= enemyData.detectRange)
        {
            isReturning = false;
            isChasing = true;
        }

        if (!isChasing && !isReturning)
        {
            HandlePatrol();
            return;
        }

        if (isReturning)
        {
            agent.isStopped = false;
            agent.speed = enemyData.moveSpeed;
            agent.SetDestination(spawnPosition);

            if (distanceToSpawn <= 0.2f)
            {
                isReturning = false;
                agent.isStopped = true;
                agent.ResetPath();
                enemyAnimation?.PlayIdle();
            }

            enemyAnimation?.SetMoveSpeed(agent.velocity.magnitude);
            return;
        }

        if (distanceToTarget <= enemyData.attackRange)
        {
            attackRecoverTimer = enemyData.attackRecoverTime;

            Vector3 lookPos = new Vector3(target.position.x, transform.position.y, target.position.z);
            transform.LookAt(lookPos);

            if (distanceToTarget > agent.stoppingDistance + 0.1f)
            {
                agent.isStopped = false;
                agent.speed = enemyData.attackSpeed;
                agent.SetDestination(target.position);
            }
            else
            {
                agent.isStopped = true;
            }

            enemyAttack?.TryAttack();
        }

        else
        {
            agent.isStopped = false;

            if (attackRecoverTimer > 0f)
            {
                attackRecoverTimer -= Time.deltaTime;
                agent.speed = enemyData.attackSpeed;
            }
            else
            {
                agent.speed = enemyData.moveSpeed;
            }

            agent.SetDestination(target.position);
        }

        float moveSpeed = agent.isStopped ? 0f : agent.velocity.magnitude;
        enemyAnimation?.SetMoveSpeed(moveSpeed);
    }

    private void ApplyData()
    {
        if (enemyData == null)
            return;

        agent.speed = enemyData.moveSpeed;
        agent.angularSpeed = 600f;
        agent.acceleration = 20f;
        agent.stoppingDistance = enemyData.attackRange;
    }

    private void HandlePatrol()
    {
        patrolTimer -= Time.deltaTime;

        // 이동 중
        if (isPatrolling)
        {
            agent.isStopped = false;
            agent.speed = enemyData.patrolSpeed;

            // 목적지 도착하면 멈춤
            if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
            {
                isPatrolling = false;
                patrolTimer = enemyData.patrolWaitTime;
                agent.isStopped = true;
            }
        }
        else
        {
            // 대기 중 → 시간 지나면 새 위치 이동
            if (patrolTimer <= 0f)
            {
                Vector3 patrolPoint = GetRandomPatrolPoint();

                agent.SetDestination(patrolPoint);
                isPatrolling = true;
            }
        }

        float moveSpeed = agent.isStopped ? 0f : agent.velocity.magnitude;
        enemyAnimation?.SetMoveSpeed(moveSpeed);
    }

    private Vector3 GetRandomPatrolPoint()
    {
        Vector2 randomCircle = Random.insideUnitCircle * enemyData.patrolRadius;
        Vector3 randomPos = spawnPosition + new Vector3(randomCircle.x, 0f, randomCircle.y);

        if (NavMesh.SamplePosition(randomPos, out NavMeshHit hit, 1.5f, NavMesh.AllAreas))
        {
            return hit.position;
        }

        return spawnPosition;
    }

    public void SetData(EnemyData data)
    {
        enemyData = data;
        ApplyData();
        enemyAttack?.SetData(data);
    }
}