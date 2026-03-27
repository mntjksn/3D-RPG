using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    private Animator animator;
    private PlayerActionLock actionLock;
    private bool isAttacking;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        actionLock = GetComponent<PlayerActionLock>();
    }

    private void Update()
    {
        if (isAttacking)
            return;

        if (actionLock == null || actionLock.CanAttack)
        {
            if (Input.GetMouseButtonDown(0))
            {
                isAttacking = true;
                animator.SetTrigger("Attack");

                if (actionLock != null)
                    actionLock.StartAttack();
            }
        }
    }

    // 공격 애니메이션 마지막에 Animation Event로 호출
    public void EndAttack()
    {
        isAttacking = false;

        if (actionLock != null)
            actionLock.EndAttack();
    }
}