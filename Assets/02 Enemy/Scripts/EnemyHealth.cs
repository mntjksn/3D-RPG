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

    private float currentHp;
    private bool isDead;

    public EnemyData EnemyData => enemyData;
    public float CurrentHp => currentHp;
    public float MaxHp => enemyData != null ? enemyData.maxHp : 0f;
    public bool IsDead => isDead;

    private void Awake()
    {
        enemyActionLock = GetComponent<EnemyActionLock>();
        enemyAnimation = GetComponent<EnemyAnimation>();
        enemyPool = GetComponent<EnemyPool>();
        enemyHealthBar = GetComponentInChildren<EnemyHealthBar>();
    }

    public void SetData(EnemyData data)
    {
        enemyData = data;
        ResetHealthState();
    }

    public void SetSpawner(EnemySpawner ownerSpawner)
    {
        enemySpawner = ownerSpawner;
    }

    public void TakeDamage(float damage)
    {
        if (!CanTakeDamage())
            return;

        ApplyDamage(damage);

        if (IsDeadByHp())
        {
            Die();
            return;
        }
    }

    private bool CanTakeDamage()
    {
        return !isDead && enemyData != null;
    }

    private void ApplyDamage(float damage)
    {
        currentHp -= damage;
        enemyHealthBar?.UpdateHealthBar(currentHp, MaxHp);
        Debug.Log($"{enemyData.enemyName} 피격! 남은 체력: {currentHp}");
    }

    private bool IsDeadByHp()
    {
        return currentHp <= 0f;
    }

    private void Die()
    {
        if (isDead)
            return;

        isDead = true;

        HandleDrops();
        PlayDieReaction();
        RequestRespawn();
        StartCoroutine(ReturnToPoolRoutine());
    }

    // 사망 처리
    private void PlayDieReaction()
    {
        enemyAnimation?.PlayDie();
        enemyActionLock?.OnDie();

        GiveExpToPlayer();
    }

    private void RequestRespawn()
    {
        enemySpawner?.RequestRespawn(enemyData);
    }

    // 체력 상태 초기화
    private void ResetHealthState()
    {
        if (enemyData == null)
            return;

        currentHp = enemyData.maxHp;
        isDead = false;

        enemyHealthBar?.UpdateHealthBar(currentHp, MaxHp);
    }

    private IEnumerator ReturnToPoolRoutine()
    {
        if (enemyData == null)
            yield break;

        yield return new WaitForSeconds(enemyData.deadBodyDuration);
        enemyPool?.ReturnToPool();
    }

    private void GiveExpToPlayer()
    {
        if (enemyData == null)
            return;

        PlayerManager.Instance.AddExp(enemyData.exp);
    }

    private void HandleDrops()
    {
        if (enemyData == null)
            return;

        int droppedGold = EnemyDropResolver.RollGold(enemyData);
        var drops = EnemyDropResolver.RollDrops(enemyData);

        DropManager.Instance?.SpawnDrops(transform.position, droppedGold, drops);
    }
}