using UnityEngine;

// 방어 판정 트리거 - 범위 내 EnemyAttack을 PlayerShield에 등록/해제
public class PlayerShieldTrigger : MonoBehaviour
{
    [SerializeField] private PlayerShield playerShield;

    // Inspector Reset 시 부모에서 PlayerShield 자동 할당
    private void Reset()
    {
        playerShield = GetComponentInParent<PlayerShield>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (playerShield == null) return;
        if (other.transform.root == transform.root) return; // 자기 자신 무시

        EnemyAttack enemyAttack = other.GetComponentInParent<EnemyAttack>();
        if (enemyAttack == null) return;

        playerShield.AddBlockTarget(enemyAttack.transform);
    }

    private void OnTriggerExit(Collider other)
    {
        if (playerShield == null) return;
        if (other.transform.root == transform.root) return;

        EnemyAttack enemyAttack = other.GetComponentInParent<EnemyAttack>();
        if (enemyAttack == null) return;

        playerShield.RemoveBlockTarget(enemyAttack.transform);
    }
}