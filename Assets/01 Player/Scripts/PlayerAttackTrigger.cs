using UnityEngine;

// 공격 판정 트리거 - 범위 진입/이탈 시 PlayerAttack에 대상 등록/해제
public class PlayerAttackTrigger : MonoBehaviour
{
    [SerializeField] private PlayerAttack playerAttack;

    // Inspector Reset 시 부모에서 PlayerAttack 자동 할당
    private void Reset()
    {
        playerAttack = GetComponentInParent<PlayerAttack>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (playerAttack == null) return;
        if (other.transform.root == transform.root) return; // 자기 자신 무시

        IDamageable damageable = other.GetComponentInParent<IDamageable>();
        if (damageable == null) return;

        playerAttack.AddTarget(damageable);
    }

    private void OnTriggerExit(Collider other)
    {
        if (playerAttack == null) return;
        if (other.transform.root == transform.root) return;

        IDamageable damageable = other.GetComponentInParent<IDamageable>();
        if (damageable == null) return;

        playerAttack.RemoveTarget(damageable);
    }
}