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
    private EnemyActionLock enemyActionLock;
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
        enemyActionLock = GetComponent<EnemyActionLock>();
        enemyAttack = GetComponent<EnemyAttack>();
    }

    private void Update()
    {
        if (!CanUpdate())
            return;

        if (HandleLockedState())
            return;

        float distanceToTarget = GetDistanceToTarget();
        float distanceToSpawn = GetDistanceToSpawn();

        UpdateStateByDistance(distanceToTarget);

        if (HandlePatrolState())
            return;

        if (HandleReturnState(distanceToTarget, distanceToSpawn))
            return;

        HandleChaseAndAttack(distanceToTarget);
        UpdateMoveAnimation();
    }

    private bool CanUpdate()
    {
        return enemyData != null &&
               target != null &&
               agent != null &&
               agent.enabled &&
               agent.isOnNavMesh;
    }

    private bool HandleLockedState()
    {
        if (enemyActionLock == null || enemyActionLock.CanMove)
            return false;

        StopAgent();
        ClearStates();
        enemyAnimation?.SetMoveSpeed(0f);
        return true;
    }

    private float GetDistanceToTarget()
    {
        return Vector3.Distance(transform.position, target.position);
    }

    private float GetDistanceToSpawn()
    {
        return Vector3.Distance(transform.position, spawnPosition);
    }

    // 추적/복귀 상태 갱신
    private void UpdateStateByDistance(float distanceToTarget)
    {
        if (!isChasing && !isReturning && distanceToTarget <= enemyData.detectRange)
        {
            isChasing = true;
            isPatrolling = false;
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
    }

    // 순찰 상태 처리
    private bool HandlePatrolState()
    {
        if (isChasing || isReturning)
            return false;

        HandlePatrol();
        return true;
    }

    // 복귀 상태 처리
    private bool HandleReturnState(float distanceToTarget, float distanceToSpawn)
    {
        if (!isReturning)
            return false;

        agent.isStopped = false;
        agent.speed = enemyData.moveSpeed;
        agent.SetDestination(spawnPosition);

        if (!agent.pathPending && distanceToSpawn <= 0.2f)
        {
            isReturning = false;
            StopAgent();
            enemyAnimation?.PlayIdle();
            enemyAnimation?.SetMoveSpeed(0f);
        }
        else
        {
            enemyAnimation?.SetMoveSpeed(agent.velocity.magnitude);
        }

        return true;
    }

    // 추적/공격 처리
    private void HandleChaseAndAttack(float distanceToTarget)
    {
        if (distanceToTarget <= enemyData.attackRange)
        {
            HandleAttackRange(distanceToTarget);
            return;
        }

        HandleChaseRange();
    }

    private void HandleAttackRange(float distanceToTarget)
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
            StopAgent();
        }

        enemyAttack?.TryAttack();
    }

    private void HandleChaseRange()
    {
        agent.isStopped = false;
        agent.speed = GetChaseSpeed();
        agent.SetDestination(target.position);
    }

    private float GetChaseSpeed()
    {
        if (attackRecoverTimer > 0f)
        {
            attackRecoverTimer -= Time.deltaTime;
            return enemyData.attackSpeed;
        }

        return enemyData.moveSpeed;
    }

    private void UpdateMoveAnimation()
    {
        float moveSpeed = agent.isStopped ? 0f : agent.velocity.magnitude;
        enemyAnimation?.SetMoveSpeed(moveSpeed);
    }

    private void StopAgent()
    {
        agent.isStopped = true;
        agent.ResetPath();
    }

    private void ClearStates()
    {
        isPatrolling = false;
        isChasing = false;
        isReturning = false;
    }

    private void FindTargetIfNeeded()
    {
        if (target != null)
            return;

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
            target = player.transform;
    }

    private void ApplyData()
    {
        if (enemyData == null || agent == null)
            return;

        agent.speed = enemyData.moveSpeed;
        agent.angularSpeed = 600f;
        agent.acceleration = 20f;
        agent.stoppingDistance = enemyData.attackRange;
    }

    // 재사용 시 상태 초기화
    private void ResetAIState()
    {
        FindTargetIfNeeded();

        spawnPosition = transform.position;

        patrolTimer = 0f;
        attackRecoverTimer = 0f;

        ClearStates();

        enemyActionLock?.ResetToSpawnState();
        enemyAnimation?.ResetAnimation();
        enemyAttack?.ResetAttackState();

        ApplyData();

        if (agent != null && agent.enabled && agent.isOnNavMesh)
            StopAgent();

        enemyAnimation?.SetMoveSpeed(0f);

        enemyAttack?.SetData(enemyData);
        enemyAttack?.SetTarget(target);
    }

    // 순찰 처리
    private void HandlePatrol()
    {
        patrolTimer -= Time.deltaTime;

        if (isPatrolling)
        {
            agent.isStopped = false;
            agent.speed = enemyData.patrolSpeed;

            if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
            {
                isPatrolling = false;
                patrolTimer = enemyData.patrolWaitTime;
                StopAgent();
                enemyAnimation?.SetMoveSpeed(0f);
            }
        }
        else
        {
            if (patrolTimer > 0f)
                return;

            Vector3 patrolPoint = GetRandomPatrolPoint();
            agent.isStopped = false;
            agent.SetDestination(patrolPoint);
            isPatrolling = true;
        }

        UpdateMoveAnimation();
    }

    private Vector3 GetRandomPatrolPoint()
    {
        Vector2 randomCircle = Random.insideUnitCircle * enemyData.patrolRadius;
        Vector3 randomPos = spawnPosition + new Vector3(randomCircle.x, 0f, randomCircle.y);

        if (NavMesh.SamplePosition(randomPos, out NavMeshHit hit, 1.5f, NavMesh.AllAreas))
            return hit.position;

        return spawnPosition;
    }

    public void SetData(EnemyData data)
    {
        enemyData = data;
        ResetAIState();
    }
}