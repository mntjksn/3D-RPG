using UnityEngine;

public class EnemyAttack : MonoBehaviour
{
    [SerializeField] private EnemyData enemyData;

    private EnemyActionLock enemyActionLock;
    private EnemyAnimation enemyAnimation;
    private Transform target;

    private float attackCooldownTimer;
    private bool isAttacking;

    public bool CanAttack => attackCooldownTimer <= 0f && !isAttacking;
    public bool IsAttacking => isAttacking;

    private void Awake()
    {
        enemyActionLock = GetComponent<EnemyActionLock>();
        enemyAnimation = GetComponent<EnemyAnimation>();
    }

    public void SetTarget(Transform targetTransform)
    {
        target = targetTransform;
    }

    public void SetData(EnemyData data)
    {
        enemyData = data;
    }

    private void Update()
    {
        attackCooldownTimer = Mathf.Max(0f, attackCooldownTimer - Time.deltaTime);
    }

    public void TryAttack()
    {
        if (enemyActionLock != null && !enemyActionLock.CanAttack)
            return;

        if (!CanAttack || target == null || enemyData == null)
            return;

        StartAttack();
    }

    private void StartAttack()
    {
        isAttacking = true;
        attackCooldownTimer = enemyData.attackRate;

        enemyActionLock?.SetAttack(true);
        enemyAnimation?.PlayAttack();
    }

    public void EndAttack()
    {
        isAttacking = false;
        enemyActionLock?.SetAttack(false);
    }

    public void CancelAttack()
    {
        isAttacking = false;
        enemyActionLock?.SetAttack(false);
    }

    // 애니메이션 이벤트로 호출
    public void DealDamage()
    {
        if (target == null) return;

        float distance = Vector3.Distance(transform.position, target.position);

        if (distance <= enemyData.attackRange + 0.5f)
        {
            //PlayerHealth player = target.GetComponent<PlayerHealth>();
            //f (player != null)
            {
               // player.TakeDamage(enemyData.attackDamage);
            }
        }
    }
}