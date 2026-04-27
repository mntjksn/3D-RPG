using Photon.Pun;
using UnityEngine;

// 적 공격 처리 - 마스터 클라이언트에서 공격 판정 및 피해 적용
public class EnemyAttack : MonoBehaviour
{
    [SerializeField] private EnemyData enemyData;

    private EnemyActionLock enemyActionLock;
    private EnemyAnimation enemyAnimation;
    private EnemyAttackNetworkSync networkSync;

    private Transform target;
    private float attackCooldownTimer;
    private bool isAttacking;

    private int mySpawnerIndex = -1;
    private int myPoolIndex = -1;
    private bool isInitialized = false;

    public int SpawnerIndex => mySpawnerIndex;
    public int PoolIndex => myPoolIndex;
    public int UniqueId => (mySpawnerIndex << 16) | myPoolIndex;
    public bool IsInitialized => isInitialized;

    // 쿨다운이 끝나고 공격 중이 아닐 때만 true
    public bool CanAttack => attackCooldownTimer <= 0f && !isAttacking;
    public bool IsAttacking => isAttacking;

    private void Awake()
    {
        enemyActionLock = GetComponent<EnemyActionLock>();
        enemyAnimation = GetComponent<EnemyAnimation>();
        networkSync = GetComponent<EnemyAttackNetworkSync>();
    }

    private void OnDisable()
    {
        isInitialized = false;
    }

    private void Update()
    {
        attackCooldownTimer = Mathf.Max(0f, attackCooldownTimer - Time.deltaTime);
    }

    public void SetTarget(Transform targetTransform)
    {
        target = targetTransform;
    }

    // 스포너/풀 인덱스 포함 전체 초기화용 오버로드
    public void SetData(EnemyData data, int sIndex, int pIndex)
    {
        enemyData = data;
        mySpawnerIndex = sIndex;
        myPoolIndex = pIndex;
        isInitialized = true;
    }

    // 데이터만 갱신하는 간단한 오버로드
    public void SetData(EnemyData data)
    {
        enemyData = data;
    }

    // 공격 상태 초기화 (부활/풀 반환 시 호출)
    public void ResetAttackState()
    {
        attackCooldownTimer = 0f;
        isAttacking = false;
        enemyActionLock?.SetAttack(false);
    }

    // 마스터 클라이언트에서 공격 시도 (EnemyAI에서 호출)
    public void TryAttack()
    {
        if (!PhotonNetwork.IsMasterClient) return;
        if (!isInitialized) return;
        if (!CanStartAttack()) return;

        StartAttack();
    }

    // 행동 잠금 여부, 쿨다운, 타겟 유효성 모두 확인
    private bool CanStartAttack()
    {
        if (enemyActionLock != null && !enemyActionLock.CanAttack)
            return false;

        return CanAttack && target != null && enemyData != null;
    }

    // 공격 시작 - 쿨다운 설정 및 전체 클라에 애니메이션 동기화
    private void StartAttack()
    {
        isAttacking = true;
        attackCooldownTimer = enemyData.attackRate;
        enemyActionLock?.SetAttack(true);
        networkSync?.BroadcastAttack();
    }

    // 네트워크 동기화를 통해 모든 클라이언트에서 호출됨
    public void PlayAttackAnimation()
    {
        enemyAnimation?.PlayAttack();
    }

    // Animation Event로 호출 - 공격 종료
    public void EndAttack()
    {
        isAttacking = false;
        enemyActionLock?.SetAttack(false);
    }

    // Animation Event로 호출 - 공격 범위 내 플레이어에게 피해 적용
    public void DealDamage()
    {
        if (!PhotonNetwork.IsMasterClient) return;
        if (!isInitialized) return;
        if (!CanDealDamage()) return;
        if (!IsTargetInRange()) return;

        ApplyDamage();
    }

    private bool CanDealDamage()
    {
        return target != null && enemyData != null;
    }

    // 타겟이 공격 범위 + 여유값 이내에 있는지 확인
    private bool IsTargetInRange()
    {
        float distance = Vector3.Distance(transform.position, target.position);
        return distance <= enemyData.attackRange + 0.5f;
    }

    // 공격 범위 내 모든 플레이어에게 RPC로 피해 전달 (방어력 감소 적용)
    private void ApplyDamage()
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");

        foreach (GameObject playerObj in players)
        {
            PlayerHealth playerHealth = playerObj.GetComponent<PlayerHealth>();
            if (playerHealth == null) continue;

            float dist = Vector3.Distance(transform.position, playerObj.transform.position);
            if (dist > enemyData.attackRange + 0.5f) continue;

            float finalDamage = playerHealth.ModifyIncomingDamage(transform, enemyData.attackDamage);
            if (finalDamage <= 0f) continue;

            playerHealth.photonView.RPC(
                nameof(PlayerHealth.RPC_TakeDamage),
                playerHealth.photonView.Owner,
                finalDamage);
        }
    }
}