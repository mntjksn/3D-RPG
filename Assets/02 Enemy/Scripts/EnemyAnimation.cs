using UnityEngine;

public class EnemyAnimation : MonoBehaviour
{
    private Animator animator;

    private readonly int hashSpeed = Animator.StringToHash("Speed");
    private readonly int hashIdle = Animator.StringToHash("Idle");
    private readonly int hashAttack = Animator.StringToHash("Attack");
    private readonly int hashHit = Animator.StringToHash("Hit");
    private readonly int hashDie = Animator.StringToHash("Die");

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    public void SetMoveSpeed(float speed)
    {
        if (animator == null) return;
        animator.SetFloat(hashSpeed, speed, 0.1f, Time.deltaTime);
    }

    public void PlayIdle()
    {
        if (animator == null) return;
        animator.SetTrigger(hashIdle);
    }

    public void PlayAttack()
    {
        if (animator == null) return;
        animator.SetTrigger(hashAttack);
    }

    public void PlayHit()
    {
        if (animator == null) return;
        animator.SetTrigger(hashHit);
    }

    public void PlayDie()
    {
        if (animator == null) return;
        animator.SetTrigger(hashDie);
    }
}