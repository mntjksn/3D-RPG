using Photon.Pun;
using System.Collections;
using UnityEngine;

// 적 HP 관리, 피해/회복 처리, 사망 시 드랍/경험치/리스폰 요청
public class EnemyHealth : MonoBehaviour, IDamageable
{
    [Header("Data")]
    [SerializeField] private EnemyData enemyData;

    private EnemyActionLock enemyActionLock;
    private EnemyAnimation enemyAnimation;
    private EnemySpawner enemySpawner;
    private EnemyPool enemyPool;
    private EnemyHealthBar enemyHealthBar;
    private HitFlash hitFlash;
    private EnemyHealthNetworkSync networkSync;

    private float currentHp;
    private bool isDead;
    private float lastHitTime;

    private int mySpawnerIndex = -1;
    private int myPoolIndex = -1;
    private bool isInitialized = false;

    // 드랍 아이템 귀속 처리에 사용
    private int lastAttackerActorNumber = -1;

    public EnemyData EnemyData => enemyData;
    public float CurrentHp => currentHp;
    public float MaxHp => enemyData != null ? enemyData.maxHp : 0f;
    public bool IsDead => isDead;
    public int UniqueId => (mySpawnerIndex << 16) | myPoolIndex;
    public bool IsInitialized => isInitialized;

    [Header("Auto Heal")]
    [SerializeField] private float regen = 0.05f;
    [SerializeField] private float healDelay = 5f; // 피격 후 자동 회복 시작까지 대기 시간

    private void Awake()
    {
        enemyActionLock = GetComponent<EnemyActionLock>();
        enemyAnimation = GetComponent<EnemyAnimation>();
        enemyPool = GetComponent<EnemyPool>();
        enemyHealthBar = GetComponentInChildren<EnemyHealthBar>();
        hitFlash = GetComponent<HitFlash>();
        networkSync = GetComponent<EnemyHealthNetworkSync>();
    }

    private void OnDisable()
    {
        isInitialized = false;
    }

    // 자동 HP 재생 - 마스터 클라이언트에서만 계산 후 동기화
    private void Update()
    {
        if (!PhotonNetwork.IsMasterClient) return;
        if (!isInitialized) return;
        if (isDead || enemyData == null) return;
        if (Time.time < lastHitTime + healDelay) return;
        if (currentHp >= MaxHp) return;

        currentHp = Mathf.Min(currentHp + MaxHp * regen * Time.deltaTime, MaxHp);

        networkSync?.BroadcastHeal(currentHp);
        enemyHealthBar?.UpdateHealthBar(currentHp, MaxHp);
    }

    // 스포너/풀 인덱스 포함 전체 초기화 (풀에서 꺼낼 때 호출)
    public void SetData(EnemyData data, int sIndex, int pIndex)
    {
        enemyData = data;
        mySpawnerIndex = sIndex;
        myPoolIndex = pIndex;
        isInitialized = true;
        ResetHealthState();
    }

    public void SetSpawner(EnemySpawner ownerSpawner)
    {
        enemySpawner = ownerSpawner;
    }

    // IDamageable 인터페이스 구현 - 공격자 정보 없을 때 로컬 플레이어로 처리
    public void TakeDamage(float damage)
    {
        TakeDamage(damage, PhotonNetwork.LocalPlayer.ActorNumber);
    }

    // 마스터면 직접 브로드캐스트, 클라이언트면 마스터에게 요청
    public void TakeDamage(float damage, int attackerActorNumber)
    {
        if (!isInitialized) return;

        if (PhotonNetwork.IsMasterClient)
        {
            if (!CanTakeDamage()) return;
            networkSync?.BroadcastDamage(damage, attackerActorNumber);
        }
        else
        {
            networkSync?.RequestDamage(damage, attackerActorNumber);
        }
    }

    // 네트워크 동기화를 통해 모든 클라이언트에서 실제 피해 적용
    public void ApplyDamage(float damage, int attackerActorNumber)
    {
        lastAttackerActorNumber = attackerActorNumber;
        currentHp -= damage;

        enemyHealthBar?.UpdateHealthBar(currentHp, MaxHp);
        SoundManager.Instance?.PlaySFX(SfxType.PlayerHit);
        hitFlash?.PlayFlash();

        lastHitTime = Time.time;

        if (currentHp <= 0f)
            Die();
    }

    // 네트워크 동기화를 통해 모든 클라이언트에서 회복값 적용
    public void ApplyHeal(float hp)
    {
        currentHp = hp;
        enemyHealthBar?.UpdateHealthBar(currentHp, MaxHp);
    }

    private bool CanTakeDamage()
    {
        return !isDead && enemyData != null;
    }

    // 사망 처리 - 콜라이더 비활성화, 애니메이션, 드랍/경험치/리스폰은 마스터만 처리
    private void Die()
    {
        if (isDead) return;

        isDead = true;

        CapsuleCollider col = GetComponent<CapsuleCollider>();
        if (col != null)
            col.enabled = false;

        enemyAnimation?.PlayDie();
        enemyActionLock?.OnDie();

        GiveExpToPlayer();
        StartCoroutine(ReturnToPoolRoutine());

        QuestService.NotifyKill(enemyData.enemyName);

        if (PhotonNetwork.IsMasterClient)
        {
            HandleDrops();
            enemySpawner?.RequestRespawn(enemyData);
        }
    }

    // HP 및 상태를 초기값으로 리셋
    private void ResetHealthState()
    {
        if (enemyData == null) return;

        currentHp = enemyData.maxHp;
        isDead = false;
        lastHitTime = Time.time;
        lastAttackerActorNumber = -1;

        enemyHealthBar?.UpdateHealthBar(currentHp, MaxHp);
    }

    // 사망 후 일정 시간 뒤 풀에 반환 (마스터만 실행)
    private IEnumerator ReturnToPoolRoutine()
    {
        yield return new WaitForSeconds(enemyData.deadBodyDuration);

        if (PhotonNetwork.IsMasterClient)
            enemyPool?.ReturnToPool();
    }

    // 로컬 플레이어에게 경험치 지급
    private void GiveExpToPlayer()
    {
        PlayerManager.Instance?.AddExp(enemyData.exp);
    }

    // 드랍 아이템 및 골드 생성 - 마지막 타격자에게 귀속
    private void HandleDrops()
    {
        if (lastAttackerActorNumber <= 0) return;

        int gold = EnemyDropResolver.RollGold(enemyData);
        var drops = EnemyDropResolver.RollDrops(enemyData);

        DropManager.Instance?.SpawnDrops(transform.position, gold, drops, lastAttackerActorNumber);
    }
}