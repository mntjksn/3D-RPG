using UnityEngine;
using Photon.Pun;

public class PlayerAnimation : MonoBehaviourPun
{
    private Animator animator;

    private readonly int hashSpeed = Animator.StringToHash("Speed");
    private readonly int hashAttack = Animator.StringToHash("Attack");
    private readonly int hashShield = Animator.StringToHash("Shield");
    private readonly int hashDie = Animator.StringToHash("Die");

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    private void Start()
    {
        // 원격 플레이어 첫 생성 시 기본 상태만 맞춤
        if (!photonView.IsMine && animator != null)
        {
            animator.Play("Idle", 0, 0f);
            animator.Update(0f);
        }
    }

    public void ResetAnimation()
    {
        if (animator == null)
            return;

        animator.Rebind();
        animator.Update(0f);

        ForceIdleState();
    }

    public void ForceIdleState()
    {
        if (animator == null)
            return;

        animator.ResetTrigger(hashAttack);
        animator.ResetTrigger(hashDie);
        animator.SetBool(hashShield, false);
        animator.SetFloat(hashSpeed, 0f);

        animator.Play("Idle", 0, 0f);
        animator.Update(0f);
    }

    public void SetMoveSpeed(float speed)
    {
        if (animator == null)
            return;

        animator.SetFloat(hashSpeed, speed, 0.1f, Time.deltaTime);
    }

    public void PlayAttack()
    {
        if (animator == null)
            return;

        animator.SetTrigger(hashAttack);
    }

    public void PlayShield(bool value)
    {
        if (animator == null)
            return;

        animator.SetBool(hashShield, value);
    }

    public void PlayDie()
    {
        if (animator == null)
            return;

        animator.ResetTrigger(hashAttack);
        animator.SetBool(hashShield, false);
        animator.SetFloat(hashSpeed, 0f);
        animator.SetTrigger(hashDie);
    }
}