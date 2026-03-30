using UnityEngine;

public class PlayerAttackTrigger : MonoBehaviour
{
    [SerializeField] private PlayerAttack playerAttack;

    private void Reset()
    {
        playerAttack = GetComponentInParent<PlayerAttack>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (playerAttack == null)
            return;

        if (other.transform.root == transform.root)
            return;

        IDamageable damageable = other.GetComponentInParent<IDamageable>();
        if (damageable == null)
            return;

        playerAttack.AddTarget(damageable);
    }

    private void OnTriggerExit(Collider other)
    {
        if (playerAttack == null)
            return;

        if (other.transform.root == transform.root)
            return;

        IDamageable damageable = other.GetComponentInParent<IDamageable>();
        if (damageable == null)
            return;

        playerAttack.RemoveTarget(damageable);
    }
}