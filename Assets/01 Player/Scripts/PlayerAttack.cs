using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

// 마우스 좌클릭 공격 입력 처리, 범위 내 가장 가까운 대상에게 피해 적용
public class PlayerAttack : MonoBehaviourPun
{
    private PlayerAnimation playerAnimation;
    private PlayerActionLock actionLock;
    private PlayerStat playerStat;

    private bool isAttacking;

    // 공격 범위 내 대상 목록 (PlayerAttackTrigger에서 관리)
    private readonly List<IDamageable> targetsInRange = new();

    private void Awake()
    {
        playerAnimation = GetComponent<PlayerAnimation>();
        actionLock = GetComponent<PlayerActionLock>();
        playerStat = GetComponent<PlayerStat>();
    }

    private void Update()
    {
        if (!photonView.IsMine) return;
        if (isAttacking || (actionLock != null && actionLock.IsShielding)) return;
        if (actionLock != null && !actionLock.CanAttack) return;

        if (Input.GetMouseButtonDown(0))
        {
            isAttacking = true;
            playerAnimation?.PlayAttack();
            actionLock?.SetAttack(true);
        }
    }

    // 범위 진입 시 PlayerAttackTrigger에서 호출
    public void AddTarget(IDamageable target)
    {
        if (!photonView.IsMine) return;
        if (target == null || targetsInRange.Contains(target)) return;
        targetsInRange.Add(target);
    }

    // 범위 이탈 시 PlayerAttackTrigger에서 호출
    public void RemoveTarget(IDamageable target)
    {
        if (!photonView.IsMine) return;
        targetsInRange.Remove(target);
    }

    // Animation Event로 호출 - 가장 가까운 대상에게 피해 적용
    public void AttackHit()
    {
        if (!photonView.IsMine) return;

        // null 참조 정리
        for (int i = targetsInRange.Count - 1; i >= 0; i--)
        {
            if (targetsInRange[i] == null)
                targetsInRange.RemoveAt(i);
        }

        if (targetsInRange.Count == 0) return;

        // 가장 가까운 대상 탐색
        IDamageable nearest = null;
        float nearestDist = float.MaxValue;

        for (int i = 0; i < targetsInRange.Count; i++)
        {
            MonoBehaviour mono = targetsInRange[i] as MonoBehaviour;
            if (mono == null) continue;

            float dist = Vector3.Distance(transform.position, mono.transform.position);
            if (dist < nearestDist)
            {
                nearestDist = dist;
                nearest = targetsInRange[i];
            }
        }

        if (nearest == null) return;

        float finalDamage = playerStat != null ? playerStat.AttackPower : 0f;
        int attackerActorNumber = photonView.OwnerActorNr;

        // 에너미는 공격자 정보 포함, 그 외는 기본 TakeDamage 호출
        if (nearest is EnemyHealth enemyHealth)
            enemyHealth.TakeDamage(finalDamage, attackerActorNumber);
        else
            nearest.TakeDamage(finalDamage);
    }

    // Animation Event로 호출 - 공격 종료
    public void EndAttack()
    {
        if (!photonView.IsMine) return;

        isAttacking = false;
        actionLock?.SetAttack(false);
    }

    // 사망 또는 부활 시 공격 상태 완전 초기화
    public void ResetAttackState()
    {
        if (!photonView.IsMine) return;

        isAttacking = false;
        targetsInRange.Clear();
        actionLock?.SetAttack(false);
    }

    // Animation Event로 호출 - 공격 사운드 재생
    public void PlayAttackSFX()
    {
        if (!photonView.IsMine) return;
        SoundManager.Instance?.PlaySFX(SfxType.PlayerAttack);
    }
}