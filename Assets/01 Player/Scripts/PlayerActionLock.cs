using UnityEngine;

// 플레이어 행동 가능 여부를 중앙에서 관리 (이동/공격/방어/UI 잠금)
public class PlayerActionLock : MonoBehaviour
{
    public bool CanMove { get; private set; }
    public bool CanAttack { get; private set; }
    public bool CanShield { get; private set; }
    public bool CanLook { get; private set; }
    public bool CanUI { get; private set; }

    public bool IsAttacking { get; private set; }
    public bool IsShielding { get; private set; }

    private void Start()
    {
        // 로드 완료 전까지 모든 조작 잠금
        LockRecoverControls();
    }

    // 모든 조작 잠금 (로딩 중, 사망 시 등)
    public void LockRecoverControls()
    {
        CanMove = CanAttack = CanLook = CanShield = CanUI = false;
    }

    // 모든 조작 해제
    public void UnlockRecoverControls()
    {
        CanMove = CanAttack = CanLook = CanShield = CanUI = true;
    }

    public void SetAttack(bool value) => IsAttacking = value;
    public void SetShield(bool value) => IsShielding = value;

    // 사망 시 호출 - 조작 잠금 및 전투 상태 초기화
    public void OnDie()
    {
        LockRecoverControls();
        IsShielding = false;
        IsAttacking = false;
    }

    // 부활 시 호출 - 전투 상태 초기화 후 조작 해제
    public void ResetState()
    {
        IsAttacking = false;
        IsShielding = false;
        UnlockRecoverControls();
    }

    // DieRecover 애니메이션 종료 시 Animation Event로 호출
    public void OnRecoverFinished()
    {
        UnlockRecoverControls();
    }
}