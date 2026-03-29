using System.Collections.Generic;
using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    [SerializeField] private float damage = 10f;

    private Animator animator;
    private PlayerActionLock actionLock;

    private bool isAttacking;

    private readonly List<IDamageable> targetsInRange = new();

    private void Awake()
    {
        animator = GetComponent<Animator>();
        actionLock = GetComponent<PlayerActionLock>();
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
                animator.SetTrigger("Attack");

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

    // 1타 타이밍에 Animation Event로 호출
    public void AttackHit()
    {
        Debug.Log("AttackHit 호출됨");
        Debug.Log("범위 안 적 수: " + targetsInRange.Count);

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

        nearestTarget?.TakeDamage(damage);
    }

    // 공격 애니메이션 마지막에 Animation Event로 호출
    public void EndAttack()
    {
        isAttacking = false;

        if (actionLock != null)
            actionLock.SetAttack(false);
    }
}