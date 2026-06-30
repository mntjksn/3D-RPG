using UnityEngine;
using Photon.Pun;

// Animator 파라미터를 래핑해 애니메이션 재생 인터페이스 제공
public class PlayerAnimation : MonoBehaviourPun
{
    private Animator animator;

    // 문자열 대신 해시값으로 파라미터 접근 (성능 최적화)
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
        // 원격 플레이어 첫 생성 시 Idle 상태로 강제 초기화
        if (!photonView.IsMine && animator != null)
        {
            animator.Play("Idle", 0, 0f);
            animator.Update(0f);
        }
    }

    // 애니메이터를 완전히 초기화하고 Idle 상태로 복귀 (부활 시 사용)
    public void ResetAnimation()
    {
        if (animator == null) return;

        animator.Rebind();
        animator.Update(0f);
        ForceIdleState();
    }

    // 모든 트리거/파라미터를 리셋하고 Idle 클립 재생
    public void ForceIdleState()
    {
        if (animator == null) return;

        animator.ResetTrigger(hashAttack);
        animator.ResetTrigger(hashDie);
        animator.SetBool(hashShield, false);
        animator.SetFloat(hashSpeed, 0f);
        animator.Play("Idle", 0, 0f);
        animator.Update(0f);
    }

    // 이동 속도를 블렌딩으로 부드럽게 반영
    public void SetMoveSpeed(float speed)
    {
        if (animator == null) return;
        animator.SetFloat(hashSpeed, speed, 0.1f, Time.deltaTime);
    }

    public void PlayAttack()
    {
        if (animator == null) return;
        animator.SetTrigger(hashAttack);
    }

    public void PlayShield(bool value)
    {
        if (animator == null) return;
        animator.SetBool(hashShield, value);
    }

    // 사망 애니메이션 - 이동/방어 상태 정리 후 트리거
    public void PlayDie()
    {
        if (animator == null) return;

        animator.ResetTrigger(hashAttack);
        animator.SetBool(hashShield, false);
        animator.SetFloat(hashSpeed, 0f);
        animator.SetTrigger(hashDie);
    }
}