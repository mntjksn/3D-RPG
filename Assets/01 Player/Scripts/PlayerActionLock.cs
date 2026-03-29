using UnityEngine;

public class PlayerActionLock : MonoBehaviour
{
    public bool CanMove { get; private set; } = false;
    public bool CanAttack { get; private set; } = false;
    public bool CanShield { get; private set; } = false;
    public bool CanLook { get; private set; } = false;

    public bool IsAttacking { get; private set; }
    public bool IsShielding { get; private set; }

    private void Start()
    {
        LockRecoverControls();
    }

    public void LockRecoverControls()
    {
        CanMove = false;
        CanAttack = false;
        CanLook = false;
        CanShield = false;
    }

    public void UnlockRecoverControls()
    {
        CanMove = true;
        CanAttack = true;
        CanLook = true;
        CanShield = true;
    }

    public void SetAttack(bool value)
    {
        IsAttacking = value;
    }

    public void SetShield(bool value)
    {
        IsShielding = value;
    }

    // DieRecover 끝날 때 Animation Event로 호출
    public void OnRecoverFinished()
    {
        UnlockRecoverControls();
    }
}