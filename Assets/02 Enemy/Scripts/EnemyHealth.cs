using System.Collections;
using UnityEngine;

public class EnemyHealth : MonoBehaviour, IDamageable
{
    [Header("Data")]
    [SerializeField] private EnemyData enemyData;

    private EnemyActionLock enemyActionLock;
    private EnemyAttack enemyAttack;
    private EnemyAnimation enemyAnimation;
    private EnemySpawner enemySpawner;

    private float hitIgnoreTimer;
    private float currentHp;
    private bool isDead;

    public float CurrentHp => currentHp;
    public float MaxHp => enemyData != null ? enemyData.maxHp : 0f;
    public bool IsDead => isDead;

    private void Awake()
    {
        enemyActionLock = GetComponent<EnemyActionLock>();
        enemyAttack = GetComponent<EnemyAttack>();
        enemyAnimation = GetComponent<EnemyAnimation>();
    }

    private void Start()
    {
        if (enemyData != null)
        {
            currentHp = enemyData.maxHp;
        }
    }

    private void Update()
    {
        if (hitIgnoreTimer > 0f)
            hitIgnoreTimer -= Time.deltaTime;
    }

    public void SetData(EnemyData data)
    {
        enemyData = data;
        currentHp = enemyData.maxHp;
        isDead = false;
        hitIgnoreTimer = 0f;
    }

    public void SetSpawner(EnemySpawner ownerSpawner)
    {
        enemySpawner = ownerSpawner;
    }


    public void TakeDamage(float damage)
    {
        if (isDead)
            return;

        currentHp -= damage;

        Debug.Log($"{gameObject.name} 피격! 남은 체력: {currentHp}");

        if (currentHp <= 0f)
        {
            Die();
            return;
        }

        // 이게 핵심
        if (hitIgnoreTimer > 0f)
            return;

        // 여기서 Data 값 사용
        hitIgnoreTimer = enemyData.attackIgnoreTime;

        enemyAnimation?.PlayHit();

        if (enemyAttack != null && enemyAttack.IsAttacking)
        {
            enemyAttack.CancelAttack();
        }
    }

    private void Die()
    {
        if (isDead)
            return;

        isDead = true;

        enemyAttack?.CancelAttack();
        enemyActionLock?.OnDie();

        Debug.Log($"{gameObject.name} 사망");

        enemyAnimation?.PlayDie();

        enemySpawner?.RequestRespawn(enemyData);

        StartCoroutine(RemoveBodyRoutine());
    }

    private IEnumerator RemoveBodyRoutine()
    {
        yield return new WaitForSeconds(enemyData.spawnTime);
        Destroy(gameObject);
    }
}