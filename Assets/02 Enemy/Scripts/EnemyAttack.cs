using Photon.Pun;
using UnityEngine;

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

    public void SetData(EnemyData data, int sIndex, int pIndex)
    {
        enemyData = data;
        mySpawnerIndex = sIndex;
        myPoolIndex = pIndex;
        isInitialized = true;
    }

    public void SetData(EnemyData data)
    {
        enemyData = data;
    }

    public void ResetAttackState()
    {
        attackCooldownTimer = 0f;
        isAttacking = false;
        enemyActionLock?.SetAttack(false);
    }

    public void TryAttack()
    {
        if (!PhotonNetwork.IsMasterClient) return;
        if (!isInitialized) return;
        if (!CanStartAttack()) return;

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

        networkSync?.BroadcastAttack();
    }

    public void PlayAttackAnimation()
    {
        enemyAnimation?.PlayAttack();
    }

    public void EndAttack()
    {
        isAttacking = false;
        enemyActionLock?.SetAttack(false);
    }

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

    private bool IsTargetInRange()
    {
        float distance = Vector3.Distance(transform.position, target.position);
        return distance <= enemyData.attackRange + 0.5f;
    }

    private void ApplyDamage()
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");

        foreach (GameObject playerObj in players)
        {
            PlayerHealth playerHealth = playerObj.GetComponent<PlayerHealth>();
            if (playerHealth == null) continue;

            float dist = Vector3.Distance(transform.position, playerObj.transform.position);
            if (dist > enemyData.attackRange + 0.5f) continue;

            float finalDamage = enemyData.attackDamage;
            finalDamage = playerHealth.ModifyIncomingDamage(transform, finalDamage);

            if (finalDamage <= 0f) continue;

            playerHealth.photonView.RPC(
                "RPC_TakeDamage",
                playerHealth.photonView.Owner,
                finalDamage
            );
        }
    }
}