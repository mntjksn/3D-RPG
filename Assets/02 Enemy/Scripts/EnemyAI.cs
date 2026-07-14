using Photon.Pun;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
// 적 AI - 마스터 클라이언트에서 순찰/추격/공격 상태를 처리하고 원격 클라이언트에 동기화
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

    // 가장 가까운 플레이어를 주기적으로 재탐색하는 타이머
    private float retargetTimer = 0f;
    private const float retargetInterval = 1f;

    // 원격 클라이언트에서 보간에 사용할 네트워크 동기화 값
    private Vector3 networkPosition;
    private Quaternion networkRotation;
    private float networkMoveSpeed;

    private int mySpawnerIndex = -1;
    private int myPoolIndex = -1;
    private bool isInitialized = false;

    public int SpawnerIndex => mySpawnerIndex;
    public int PoolIndex => myPoolIndex;
    public int UniqueId => (mySpawnerIndex << 16) | myPoolIndex; // 스포너+풀 인덱스로 고유 ID 생성
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
            UpdateMasterAI();
        else
            UpdateRemoteAI();
    }

    // 마스터 클라이언트 전용 - 타겟 탐색, 상태 전환, 이동/공격 처리
    private void UpdateMasterAI()
    {
        retargetTimer -= Time.deltaTime;
        if (target == null || !target.gameObject.activeInHierarchy || retargetTimer <= 0f)
        {
            retargetTimer = retargetInterval;
            FindNearestTarget();
        }

        // 원격 클라이언트였다가 마스터로 전환된 경우 NavMeshAgent 복구
        if (agent != null && !agent.enabled)
        {
            agent.enabled = true;
            networkPosition = Vector3.zero;
            networkRotation = Quaternion.identity;
            networkMoveSpeed = 0f;
            enemyAttack?.RefreshCachedPlayers();
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
            float distToTarget = GetDistanceToTarget();
            float distToSpawn = GetDistanceToSpawn();

            UpdateStateByDistance(distToTarget);

            if (!HandlePatrolState())
            {
                if (!HandleReturnState(distToTarget, distToSpawn))
                    HandleChaseAndAttack(distToTarget);
            }
        }

        UpdateMoveAnimation();

        networkSync?.TrySyncState(
            transform.position,
            transform.rotation,
            agent.isStopped ? 0f : agent.velocity.magnitude);
    }

    // 원격 클라이언트 전용 - 네트워크 동기화 값으로 위치/애니메이션 보간
    private void UpdateRemoteAI()
    {
        if (!isInitialized) return;

        // 원격에서는 NavMeshAgent 비활성화 (마스터가 이동 제어)
        if (agent != null && agent.enabled)
            agent.enabled = false;

        if (networkPosition != Vector3.zero)
        {
            float dist = Vector3.Distance(transform.position, networkPosition);

            if (dist > 3f)
            {
                // 오차가 너무 크면 즉시 스냅
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

    // 네트워크 동기화 데이터 수신 (EnemyAINetworkSync에서 호출)
    public void ApplyNetworkState(Vector3 position, Quaternion rotation, float moveSpeed)
    {
        networkPosition = position;
        networkRotation = rotation;
        networkMoveSpeed = moveSpeed;
    }

    // 태그가 "Player"인 오브젝트 중 가장 가까운 대상을 타겟으로 설정
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

    // 행동 잠금 상태이면 이동 중단 후 true 반환
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

    // 거리에 따라 추격/귀환 상태 전환
    private void UpdateStateByDistance(float distToTarget)
    {
        // 탐지 범위 진입 시 추격 시작
        if (!isChasing && !isReturning && distToTarget <= enemyData.detectRange)
        {
            isChasing = true;
            isPatrolling = false;
        }

        // 이탈 범위 벗어나면 스폰 위치로 귀환
        if (isChasing && distToTarget >= enemyData.loseRange)
        {
            isChasing = false;
            isReturning = true;
            agent.isStopped = false;
            agent.SetDestination(spawnPosition);
        }

        // 귀환 중 다시 탐지 범위 안에 들어오면 추격 재개
        if (isReturning && distToTarget <= enemyData.detectRange)
        {
            isReturning = false;
            isChasing = true;
        }
    }

    // 순찰 상태면 순찰 처리 후 true 반환
    private bool HandlePatrolState()
    {
        if (isChasing || isReturning) return false;
        HandlePatrol();
        return true;
    }

    // 귀환 상태 처리 - 스폰 위치에 도착하면 귀환 종료
    private bool HandleReturnState(float distToTarget, float distToSpawn)
    {
        if (!isReturning) return false;

        agent.isStopped = false;
        agent.speed = enemyData.moveSpeed;
        agent.SetDestination(spawnPosition);

        if (!agent.pathPending && distToSpawn <= 0.2f)
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

    // 공격 범위 내면 공격, 아니면 추격
    private void HandleChaseAndAttack(float distToTarget)
    {
        if (distToTarget <= enemyData.attackRange)
            HandleAttackRange(distToTarget);
        else
            HandleChaseRange();
    }

    // 공격 범위 내 처리 - 타겟을 바라보고 공격 시도
    private void HandleAttackRange(float distToTarget)
    {
        attackRecoverTimer = enemyData.attackRecoverTime;

        // Y축 회전만 적용해 수평으로 타겟을 바라봄
        Vector3 lookPos = new Vector3(target.position.x, transform.position.y, target.position.z);
        transform.LookAt(lookPos);

        if (distToTarget > agent.stoppingDistance + 0.1f)
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

    // 추격 범위 처리 - 타겟 방향으로 이동
    private void HandleChaseRange()
    {
        agent.isStopped = false;
        agent.speed = GetChaseSpeed();
        agent.SetDestination(target.position);
    }

    // 공격 후 경직 시간 동안 공격 속도로 이동, 이후 일반 이동 속도 반환
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

    // NavMeshAgent에 EnemyData 값 적용
    private void ApplyData()
    {
        if (enemyData == null || agent == null) return;

        agent.speed = enemyData.moveSpeed;
        agent.angularSpeed = 600f;
        agent.acceleration = 20f;
        agent.stoppingDistance = enemyData.attackRange;
    }

    // 오브젝트 풀에서 꺼낼 때 AI 상태 전체 초기화
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
        enemyAttack?.RefreshCachedPlayers(); // 초기화 시 한 번 캐싱
    }

    // 순찰 처리 - 대기 후 랜덤 위치로 이동 반복
    private void HandlePatrol()
    {
        patrolTimer -= Time.deltaTime;

        if (isPatrolling)
        {
            agent.isStopped = false;
            agent.speed = enemyData.patrolSpeed;

            // 목적지 도착 시 대기 후 다시 순찰
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

    // 스폰 위치 기준 반경 내 NavMesh 위의 랜덤 순찰 지점 반환
    private Vector3 GetRandomPatrolPoint()
    {
        Vector2 randomCircle = Random.insideUnitCircle * enemyData.patrolRadius;
        Vector3 randomPos = spawnPosition + new Vector3(randomCircle.x, 0f, randomCircle.y);

        if (NavMesh.SamplePosition(randomPos, out NavMeshHit hit, 1.5f, NavMesh.AllAreas))
            return hit.position;

        return spawnPosition;
    }

    // 외부(EnemySpawner/Pool)에서 초기화 시 호출
    public void SetData(EnemyData data, int sIndex, int pIndex)
    {
        enemyData = data;
        mySpawnerIndex = sIndex;
        myPoolIndex = pIndex;
        isInitialized = true;
        ResetAIState();
    }
}