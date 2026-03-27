using UnityEngine;

public class EnemyAttack : MonoBehaviour
{
    [SerializeField] private EnemyData enemyData;

    private EnemyAnimation enemyAnimation;
    private Transform target;

    private float attackCooldownTimer;
    private bool isAttacking;

    public bool CanAttack => attackCooldownTimer <= 0f && !isAttacking;

    private void Awake()
    {
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
        attackCooldownTimer -= Time.deltaTime;
    }

    public void TryAttack()
    {
        if (!CanAttack || target == null || enemyData == null)
            return;

        StartAttack();
    }

    private void StartAttack()
    {
        isAttacking = true;
        attackCooldownTimer = enemyData.attackRate;

        enemyAnimation?.PlayAttack();
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

    // 애니메이션 끝에서 호출
    public void EndAttack()
    {
        isAttacking = false;
    }
}