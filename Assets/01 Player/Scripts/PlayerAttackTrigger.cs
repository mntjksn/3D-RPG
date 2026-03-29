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
        Debug.Log("들어옴: " + other.name);

        IDamageable damageable = other.GetComponentInParent<IDamageable>();
        if (damageable != null)
        {
            Debug.Log("데미지 대상 등록: " + other.name);
            playerAttack.AddTarget(damageable);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        Debug.Log("나감: " + other.name);

        IDamageable damageable = other.GetComponentInParent<IDamageable>();
        if (damageable != null)
        {
            playerAttack.RemoveTarget(damageable);
        }
    }
}