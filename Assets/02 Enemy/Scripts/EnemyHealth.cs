using Photon.Pun;
using System.Collections;
using UnityEngine;

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

    // 마지막 타격자
    private int lastAttackerActorNumber = -1;

    public EnemyData EnemyData => enemyData;
    public float CurrentHp => currentHp;
    public float MaxHp => enemyData != null ? enemyData.maxHp : 0f;
    public bool IsDead => isDead;
    public int UniqueId => (mySpawnerIndex << 16) | myPoolIndex;
    public bool IsInitialized => isInitialized;

    [Header("Auto Heal")]
    [SerializeField] private float regen = 0.05f;
    [SerializeField] private float healDelay = 5f;

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

    private void Update()
    {
        if (!PhotonNetwork.IsMasterClient) return;
        if (!isInitialized) return;
        if (isDead || enemyData == null) return;
        if (Time.time < lastHitTime + healDelay) return;
        if (currentHp >= MaxHp) return;

        float healAmount = MaxHp * regen * Time.deltaTime;
        currentHp = Mathf.Min(currentHp + healAmount, MaxHp);

        networkSync?.BroadcastHeal(currentHp);
        enemyHealthBar?.UpdateHealthBar(currentHp, MaxHp);
    }

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

    // 기존 인터페이스용
    public void TakeDamage(float damage)
    {
        TakeDamage(damage, PhotonNetwork.LocalPlayer.ActorNumber);
    }

    // 실제 사용용
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

    public void ApplyDamage(float damage, int attackerActorNumber)
    {
        lastAttackerActorNumber = attackerActorNumber;
        currentHp -= damage;

        enemyHealthBar?.UpdateHealthBar(currentHp, MaxHp);
        SoundManager.Instance.PlaySFX(SfxType.PlayerHit);
        hitFlash?.PlayFlash();

        lastHitTime = Time.time;

        if (currentHp <= 0f)
            Die();
    }

    public void ApplyHeal(float hp)
    {
        currentHp = hp;
        enemyHealthBar?.UpdateHealthBar(currentHp, MaxHp);
    }

    private bool CanTakeDamage()
    {
        return !isDead && enemyData != null;
    }

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

        if (PhotonNetwork.IsMasterClient)
        {
            QuestService.NotifyKill(enemyData.enemyName);
            HandleDrops();
            enemySpawner?.RequestRespawn(enemyData);
        }
    }

    private void ResetHealthState()
    {
        if (enemyData == null) return;

        currentHp = enemyData.maxHp;
        isDead = false;
        lastHitTime = Time.time;
        lastAttackerActorNumber = -1;

        enemyHealthBar?.UpdateHealthBar(currentHp, MaxHp);
    }

    private IEnumerator ReturnToPoolRoutine()
    {
        yield return new WaitForSeconds(enemyData.deadBodyDuration);

        if (PhotonNetwork.IsMasterClient)
            enemyPool?.ReturnToPool();
    }

    private void GiveExpToPlayer()
    {
        if (PlayerManager.Instance == null) return;
        PlayerManager.Instance.AddExp(enemyData.exp);
    }

    private void HandleDrops()
    {
        int gold = EnemyDropResolver.RollGold(enemyData);
        var drops = EnemyDropResolver.RollDrops(enemyData);

        // 마지막 타격자 없으면 드랍 생성 안 하거나, 원하면 마스터에게 주는 fallback 가능
        if (lastAttackerActorNumber <= 0)
            return;

        DropManager.Instance?.SpawnDrops(transform.position, gold, drops, lastAttackerActorNumber);
    }
}