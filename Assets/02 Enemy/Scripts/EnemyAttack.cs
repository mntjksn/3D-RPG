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

    private void Update()
    {
        UpdateCooldown();
    }

    private void UpdateCooldown()
    {
        attackCooldownTimer = Mathf.Max(0f, attackCooldownTimer - Time.deltaTime);
    }

    public void SetTarget(Transform targetTransform)
    {
        target = targetTransform;
    }

    public void SetData(EnemyData data)
    {
        enemyData = data;
    }

    // 공격 상태 초기화
    public void ResetAttackState()
    {
        attackCooldownTimer = 0f;
        isAttacking = false;
        enemyActionLock?.SetAttack(false);
    }

    public void TryAttack()
    {
        if (!CanStartAttack())
            return;

        StartAttack();
    }

    private bool CanStartAttack()
    {
        if (enemyActionLock != null && !enemyActionLock.CanAttack)
            return false;

        return CanAttack && target != null && enemyData != null;
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
        FinishAttack();
    }

    private void FinishAttack()
    {
        isAttacking = false;
        enemyActionLock?.SetAttack(false);
    }

    // 애니메이션 이벤트로 호출
    public void DealDamage()
    {
        if (!CanDealDamage())
            return;

        if (!IsTargetInRange())
            return;

        ApplyDamage();
    }

    private bool CanDealDamage()
    {
        return target != null && enemyData != null;
    }

    private bool IsTargetInRange()
    {
        float distance = Vector3.Distance(transform.position, target.position);
        return distance <= enemyData.attackRange + 0.5f;
    }

    private void ApplyDamage()
    {
        if (target == null)
            return;

        IDamageable damageable = target.GetComponentInParent<IDamageable>();
        if (damageable == null)
            return;

        float finalDamage = enemyData.attackDamage;

        PlayerHealth playerHealth = target.GetComponentInParent<PlayerHealth>();
        if (playerHealth != null)
        {
            finalDamage = playerHealth.ModifyIncomingDamage(transform, finalDamage);
        }

        if (finalDamage <= 0f)
            return;

        damageable.TakeDamage(finalDamage);
    }
}