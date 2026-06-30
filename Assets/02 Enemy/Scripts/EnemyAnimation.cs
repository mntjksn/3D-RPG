using UnityEngine;

// 적 Animator 파라미터를 래핑해 애니메이션 재생 인터페이스 제공
public class EnemyAnimation : MonoBehaviour
{
    private Animator animator;

    // 문자열 대신 해시값으로 파라미터 접근 (성능 최적화)
    private readonly int hashSpeed = Animator.StringToHash("Speed");
    private readonly int hashIdle = Animator.StringToHash("Idle");
    private readonly int hashAttack = Animator.StringToHash("Attack");
    private readonly int hashDie = Animator.StringToHash("Die");

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    // 오브젝트 풀에서 재사용 시 애니메이터 완전 초기화
    public void ResetAnimation()
    {
        if (animator == null) return;
        animator.Rebind();
        animator.Update(0f);
    }

    // 이동 속도를 블렌딩으로 부드럽게 반영
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

    public void PlayDie()
    {
        if (animator == null) return;
        animator.SetTrigger(hashDie);
    }
}