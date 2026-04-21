using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class PlayerShield : MonoBehaviourPun
{
    private PlayerAnimation playerAnimation;
    private PlayerActionLock actionLock;
    private bool isShielding;
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

        if (actionLock != null && !actionLock.CanShield && !isShielding)
            return;

        if (Input.GetMouseButton(1))
        {
            if (!isShielding)
            {
                isShielding = true;
                playerAnimation?.PlayShield(true);
                actionLock?.SetShield(true);
            }
        }
        else
        {
            if (isShielding)
            {
                isShielding = false;
                playerAnimation?.PlayShield(false);
                actionLock?.SetShield(false);
            }
        }
    }

    public void AddBlockTarget(Transform target)
    {
        if (!photonView.IsMine) return;
        if (target == null) return;
        blockersInRange.Add(target.root);
    }

    public void RemoveBlockTarget(Transform target)
    {
        if (!photonView.IsMine) return;
        if (target == null) return;
        blockersInRange.Remove(target.root);
    }

    public bool CanBlock(Transform attacker)
    {
        if (!isShielding || attacker == null) return false;
        return blockersInRange.Contains(attacker.root);
    }

    public void ResetShieldState()
    {
        if (!photonView.IsMine) return;
        isShielding = false;
        blockersInRange.Clear();
        playerAnimation?.PlayShield(false);
        actionLock?.SetShield(false);
    }
}