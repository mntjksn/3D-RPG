using UnityEngine;

public class EnemyActionLock : MonoBehaviour
{
    public bool CanMove { get; private set; } = false;
    public bool CanAttack { get; private set; } = false;

    public bool IsMoving { get; private set; }
    public bool IsAttacking { get; private set; }

    private void Start()
    {
        LockRecoverControls();
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
    }

    // DieRecover 끝날 때 Animation Event로 호출
    public void OnRecoverFinished()
    {
        UnlockRecoverControls();
    }
}
