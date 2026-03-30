using System.Collections.Generic;
using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    private PlayerAnimation playerAnimation;
    private PlayerActionLock actionLock;
    private PlayerStat playerStat;

    private bool isAttacking;

    private readonly List<IDamageable> targetsInRange = new();

    private void Awake()
    {
        playerAnimation = GetComponent<PlayerAnimation>();
        actionLock = GetComponent<PlayerActionLock>();
        playerStat = GetComponent<PlayerStat>();
    }

    private void Update()
    {
        if (isAttacking || (actionLock != null && actionLock.IsShielding))
            return;

        if (actionLock == null || actionLock.CanAttack)
        {
            if (Input.GetMouseButtonDown(0))
            {
                isAttacking = true;
                playerAnimation?.PlayAttack();

                if (actionLock != null)
                    actionLock.SetAttack(true);
            }
        }
    }

    public void AddTarget(IDamageable target)
    {
        if (target == null || targetsInRange.Contains(target))
            return;

        targetsInRange.Add(target);
    }

    public void RemoveTarget(IDamageable target)
    {
        if (target == null)
            return;

        targetsInRange.Remove(target);
    }

    public void AttackHit()
    {
        for (int i = targetsInRange.Count - 1; i >= 0; i--)
        {
            if (targetsInRange[i] == null)
                targetsInRange.RemoveAt(i);
        }

        if (targetsInRange.Count == 0)
            return;

        IDamageable nearestTarget = null;
        float nearestDistance = float.MaxValue;

        for (int i = 0; i < targetsInRange.Count; i++)
        {
            MonoBehaviour targetMono = targetsInRange[i] as MonoBehaviour;
            if (targetMono == null)
                continue;

            float distance = Vector3.Distance(transform.position, targetMono.transform.position);
            if (distance < nearestDistance)
            {
                nearestDistance = distance;
                nearestTarget = targetsInRange[i];
            }
        }

        float finalDamage = playerStat != null ? playerStat.AttackPower : 0f;
        nearestTarget?.TakeDamage(finalDamage);
    }

    public void EndAttack()
    {
        isAttacking = false;

        if (actionLock != null)
            actionLock.SetAttack(false);
    }

    public void ResetAttackState()
    {
        isAttacking = false;
        targetsInRange.Clear();

        if (actionLock != null)
            actionLock.SetAttack(false);
    }
}