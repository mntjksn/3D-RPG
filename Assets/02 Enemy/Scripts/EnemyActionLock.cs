using UnityEngine;

public class EnemyActionLock : MonoBehaviour
{
    public bool CanMove { get; private set; }
    public bool CanAttack { get; private set; }

    public bool IsMoving { get; private set; }
    public bool IsAttacking { get; private set; }

    public void ResetToSpawnState()
    {
        // 생성/부활 직후: 부활 애니메이션 끝나기 전까지 잠금
        CanMove = false;
        CanAttack = false;

        IsMoving = false;
        IsAttacking = false;
    }

    public void LockRecoverControls()
    {
        CanMove = false;
        CanAttack = false;
    }

    public void UnlockRecoverControls()
    {
        CanMove = true;
        CanAttack = true;
    }

    public void SetMove(bool value)
    {
        IsMoving = value;
    }

    public void SetAttack(bool value)
    {
        IsAttacking = value;
    }

    public void OnDie()
    {
        LockRecoverControls();
        IsMoving = false;
        IsAttacking = false;
    }

    // 부활 애니메이션 끝날 때 Animation Event
    public void OnRecoverFinished()
    {
        UnlockRecoverControls();
    }
}