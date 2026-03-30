using UnityEngine;

public class PlayerShieldTrigger : MonoBehaviour
{
    [SerializeField] private PlayerShield playerShield;

    private void Reset()
    {
        playerShield = GetComponentInParent<PlayerShield>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (playerShield == null)
            return;

        if (other.transform.root == transform.root)
            return;

        EnemyAttack enemyAttack = other.GetComponentInParent<EnemyAttack>();
        if (enemyAttack == null)
            return;

        playerShield.AddBlockTarget(enemyAttack.transform);
    }

    private void OnTriggerExit(Collider other)
    {
        if (playerShield == null)
            return;

        if (other.transform.root == transform.root)
            return;

        EnemyAttack enemyAttack = other.GetComponentInParent<EnemyAttack>();
        if (enemyAttack == null)
            return;

        playerShield.RemoveBlockTarget(enemyAttack.transform);
    }
}