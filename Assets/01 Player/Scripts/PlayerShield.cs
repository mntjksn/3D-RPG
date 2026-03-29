using UnityEngine;

public class PlayerShield : MonoBehaviour
{
    private Animator animator;
    private PlayerActionLock actionLock;

    private bool isShielding;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        actionLock = GetComponent<PlayerActionLock>();
    }

    private void Update()
    {
        if (actionLock != null && !actionLock.CanAttack && !isShielding)
            return;

        if (Input.GetMouseButton(1))
        {
            if (!isShielding)
            {
                isShielding = true;
                animator.SetBool("Shield", true);

                if (actionLock != null)
                    actionLock.SetShield(true);
            }
        }
        else
        {
            if (isShielding)
            {
                isShielding = false;
                animator.SetBool("Shield", false);

                if (actionLock != null)
                    actionLock.SetShield(false);
            }
        }
    }
}