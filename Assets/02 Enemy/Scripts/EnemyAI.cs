using Photon.Pun;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class EnemyAI : MonoBehaviour
{
    [Header("Data")]
    [SerializeField] private EnemyData enemyData;
    [SerializeField] private Transform target;

    private NavMeshAgent agent;
    private EnemyAnimation enemyAnimation;
    private EnemyActionLock enemyActionLock;
    private EnemyAttack enemyAttack;
    private EnemyAINetworkSync networkSync;

    private float patrolTimer;
    private float attackRecoverTimer;
    private Vector3 spawnPosition;

    private bool isPatrolling;
    private bool isChasing;
    private bool isReturning;

    private float retargetTimer = 0f;
    private const float retargetInterval = 1f;

    private Vector3 networkPosition;
    private Quaternion networkRotation;
    private float networkMoveSpeed;

    private int mySpawnerIndex = -1;
    private int myPoolIndex = -1;
    private bool isInitialized = false;

    public int SpawnerIndex => mySpawnerIndex;
    public int PoolIndex => myPoolIndex;
    public int UniqueId => (mySpawnerIndex << 16) | myPoolIndex;
    public bool IsInitialized => isInitialized;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        enemyAnimation = GetComponent<EnemyAnimation>();
        enemyActionLock = GetComponent<EnemyActionLock>();
        enemyAttack = GetComponent<EnemyAttack>();
        networkSync = GetComponent<EnemyAINetworkSync>();
    }

    private void OnDisable()
    {
        isInitialized = false;
    }

    private void Update()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            UpdateMasterAI();
        }
        else
        {
            UpdateRemoteAI();
        }
    }

    private void UpdateMasterAI()
    {
        retargetTimer -= Time.deltaTime;
        if (target == null || !target.gameObject.activeInHierarchy || retargetTimer <= 0f)
        {
            retargetTimer = retargetInterval;
            FindNearestTarget();
        }

        if (enemyData == null || agent == null || !agent.enabled || !agent.isOnNavMesh)
            return;

        if (HandleLockedState())
            return;

        if (target == null || !target.gameObject.activeInHierarchy)
        {
            HandlePatrol();
        }
        else
        {
            float distanceToTarget = GetDistanceToTarget();
            float distanceToSpawn = GetDistanceToSpawn();

            UpdateStateByDistance(distanceToTarget);

            if (!HandlePatrolState())
            {
                if (!HandleReturnState(distanceToTarget, distanceToSpawn))
                    HandleChaseAndAttack(distanceToTarget);
            }
        }

        UpdateMoveAnimation();

        if (networkSync != null)
            networkSync.TrySyncState(transform.position, transform.rotation, agent.isStopped ? 0f : agent.velocity.magnitude);
    }

    private void UpdateRemoteAI()
    {
        if (!isInitialized)
            return;

        if (agent != null && agent.enabled)
            agent.enabled = false;

        if (networkPosition != Vector3.zero)
        {
            float dist = Vector3.Distance(transform.position, networkPosition);

            if (dist > 3f)
            {
                transform.position = networkPosition;
                transform.rotation = networkRotation;
            }
            else
            {
                transform.position = Vector3.MoveTowards(
                    transform.position,
                    networkPosition,
                    (networkMoveSpeed + 1f) * Time.deltaTime);

                if (networkMoveSpeed > 0.1f)
                {
                    transform.rotation = Quaternion.Lerp(
                        transform.rotation,
                        networkRotation,
                        Time.deltaTime * 15f);
                }
            }
        }

        enemyAnimation?.SetMoveSpeed(networkMoveSpeed);
    }

    public void ApplyNetworkState(Vector3 position, Quaternion rotation, float moveSpeed)
    {
        networkPosition = position;
        networkRotation = rotation;
        networkMoveSpeed = moveSpeed;
    }

    private void FindNearestTarget()
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        if (players.Length == 0)
        {
            target = null;
            enemyAttack?.SetTarget(null);
            return;
        }

        float minDist = float.MaxValue;
        Transform nearest = null;

        foreach (GameObject player in players)
        {
            if (!player.activeInHierarchy) continue;

            float dist = Vector3.Distance(transform.position, player.transform.position);
            if (dist < minDist)
            {
                minDist = dist;
                nearest = player.transform;
            }
        }

        target = nearest;
        enemyAttack?.SetTarget(target);
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

    private float GetDistanceToTarget() => Vector3.Distance(transform.position, target.position);
    private float GetDistanceToSpawn() => Vector3.Distance(transform.position, spawnPosition);

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

    private bool HandlePatrolState()
    {
        if (isChasing || isReturning) return false;
        HandlePatrol();
        return true;
    }

    private bool HandleReturnState(float distanceToTarget, float distanceToSpawn)
    {
        if (!isReturning) return false;

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

    private void ApplyData()
    {
        if (enemyData == null || agent == null) return;

        agent.speed = enemyData.moveSpeed;
        agent.angularSpeed = 600f;
        agent.acceleration = 20f;
        agent.stoppingDistance = enemyData.attackRange;
    }

    private void ResetAIState()
    {
        if (agent != null && !agent.enabled)
            agent.enabled = true;

        FindNearestTarget();
        spawnPosition = transform.position;

        patrolTimer = 0f;
        attackRecoverTimer = 0f;
        retargetTimer = 0f;

        networkPosition = Vector3.zero;
        networkRotation = Quaternion.identity;
        networkMoveSpeed = 0f;

        ClearStates();

        enemyActionLock?.ResetToSpawnState();
        enemyAnimation?.ResetAnimation();
        enemyAttack?.ResetAttackState();

        ApplyData();

        if (agent != null && agent.enabled && agent.isOnNavMesh)
            StopAgent();

        enemyAnimation?.SetMoveSpeed(0f);
        enemyAttack?.SetData(enemyData, mySpawnerIndex, myPoolIndex);
        enemyAttack?.SetTarget(target);
    }

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
            if (patrolTimer > 0f) return;

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

    public void SetData(EnemyData data, int sIndex, int pIndex)
    {
        enemyData = data;
        mySpawnerIndex = sIndex;
        myPoolIndex = pIndex;
        isInitialized = true;
        ResetAIState();
    }
}