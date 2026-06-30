using UnityEngine;

// 적 행동 가능 여부를 중앙에서 관리 (이동/공격 잠금)
public class EnemyActionLock : MonoBehaviour
{
    public bool CanMove { get; private set; }
    public bool CanAttack { get; private set; }
    public bool IsMoving { get; private set; }
    public bool IsAttacking { get; private set; }

    // 생성/부활 직후 - 부활 애니메이션이 끝나기 전까지 모든 행동 잠금
    public void ResetToSpawnState()
    {
        CanMove = CanAttack = IsMoving = IsAttacking = false;
    }

    // 이동/공격 잠금 (사망, 피격 경직 등)
    public void LockRecoverControls()
    {
        CanMove = CanAttack = false;
    }

    // 이동/공격 해제
    public void UnlockRecoverControls()
    {
        CanMove = CanAttack = true;
    }

    public void SetMove(bool value) => IsMoving = value;
    public void SetAttack(bool value) => IsAttacking = value;

    // 사망 시 호출 - 행동 잠금 및 상태 초기화
    public void OnDie()
    {
        LockRecoverControls();
        IsMoving = false;
        IsAttacking = false;
    }

    // 부활 애니메이션 종료 시 Animation Event로 호출
    public void OnRecoverFinished()
    {
        UnlockRecoverControls();
    }
}