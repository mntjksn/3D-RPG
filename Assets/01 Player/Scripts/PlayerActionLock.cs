using UnityEngine;

public class PlayerActionLock : MonoBehaviour
{
    public bool CanMove { get; private set; } = false;
    public bool CanAttack { get; private set; } = false;
    public bool CanLook { get; private set; } = false;
    public bool IsAttacking { get; private set; }

    private void Start()
    {
        LockRecoverControls();
    }

    public void LockRecoverControls()
    {
        CanMove = false;
        CanAttack = false;
        CanLook = false;
    }

    public void UnlockRecoverControls()
    {
        CanMove = true;
        CanAttack = true;
        CanLook = true;
    }

    public void SetMove(bool value)
    {
        CanMove = value;
    }

    public void SetAttack(bool value)
    {
        CanAttack = value;
    }

    public void SetLook(bool value)
    {
        CanLook = value;
    }

    public void StartAttack()
    {
        IsAttacking = true;
    }

    public void EndAttack()
    {
        IsAttacking = false;
    }

    // DieRecover 끝날 때 Animation Event로 호출
    public void OnRecoverFinished()
    {
        UnlockRecoverControls();
    }
}