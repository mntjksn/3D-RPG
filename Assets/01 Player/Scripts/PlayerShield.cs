using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

// 마우스 우클릭으로 방어 상태 전환, 범위 내 공격자만 막을 수 있음
public class PlayerShield : MonoBehaviourPun
{
    private PlayerAnimation playerAnimation;
    private PlayerActionLock actionLock;

    private bool isShielding;

    // 방어 범위 내 적 루트 Transform 목록
    private readonly HashSet<Transform> blockersInRange = new();

    public bool IsShielding => isShielding;

    private void Awake()
    {
        playerAnimation = GetComponent<PlayerAnimation>();
        actionLock = GetComponent<PlayerActionLock>();
    }

    private void Update()
    {
        if (!photonView.IsMine) return;
        if (actionLock != null && !actionLock.CanShield && !isShielding) return;

        if (Input.GetMouseButton(1))
        {
            if (!isShielding)
            {
                isShielding = true;
                playerAnimation?.PlayShield(true);
                actionLock?.SetShield(true);
                photonView.RPC(nameof(RPC_SyncShield), RpcTarget.Others, true);
            }
        }
        else
        {
            if (isShielding)
            {
                isShielding = false;
                playerAnimation?.PlayShield(false);
                actionLock?.SetShield(false);
                photonView.RPC(nameof(RPC_SyncShield), RpcTarget.Others, false);
            }
        }
    }

    [PunRPC]
    private void RPC_SyncShield(bool shielding)
    {
        isShielding = shielding;
    }

    // ShieldTrigger에서 호출 - 방어 가능 대상 등록
    public void AddBlockTarget(Transform target)
    {
        if (target == null) return;
        blockersInRange.Add(target.root);
    }

    // ShieldTrigger에서 호출 - 방어 가능 대상 해제
    public void RemoveBlockTarget(Transform target)
    {
        if (target == null) return;
        blockersInRange.Remove(target.root);
    }

    // 방어 중이고 공격자가 범위 내에 있을 때만 true 반환
    public bool CanBlock(Transform attacker)
    {
        if (!isShielding || attacker == null) return false;
        return blockersInRange.Contains(attacker.root);
    }

    // 사망 또는 부활 시 방어 상태 완전 초기화
    public void ResetShieldState()
    {
        if (!photonView.IsMine) return;

        if (isShielding)
            photonView.RPC(nameof(RPC_SyncShield), RpcTarget.Others, false);

        isShielding = false;
        blockersInRange.Clear();
        playerAnimation?.PlayShield(false);
        actionLock?.SetShield(false);
    }
}