using System.Collections;
using UnityEngine;

public class PlayerHealth : MonoBehaviour, IDamageable
{
    private PlayerStat playerStat;
    private PlayerAnimation playerAnimation;
    private PlayerActionLock playerActionLock;
    private CharacterController characterController;
    private PlayerAttack playerAttack;
    private PlayerShield playerShield;
    private PlayerHealthBar playerHealthBar;

    private bool isDead;
    private Vector3 respawnPosition;

    [SerializeField] private float respawnDelay = 3f;

    public bool IsDead => isDead;

    private void Awake()
    {
        playerStat = GetComponent<PlayerStat>();
        playerAnimation = GetComponent<PlayerAnimation>();
        playerActionLock = GetComponent<PlayerActionLock>();
        characterController = GetComponent<CharacterController>();
        playerAttack = GetComponent<PlayerAttack>();
        playerShield = GetComponent<PlayerShield>();
        playerHealthBar = GetComponentInChildren<PlayerHealthBar>();
    }

    private void Start()
    {
        respawnPosition = transform.position;
    }

    public void TakeDamage(float damage)
    {
        if (!CanTakeDamage())
            return;

        ApplyDamage(damage);

        if (IsDeadByHp())
        {
            Die();
            return;
        }

        PlayHitReaction();
    }

    public float ModifyIncomingDamage(Transform attacker, float damage)
    {
        if (isDead)
            return 0f;

        if (playerShield != null && playerShield.CanBlock(attacker))
        {
            float shieldPower = playerStat.GetShieldPower();
            float reducedDamage = damage * (1f - shieldPower / 100f);
            Debug.Log($"방패로 피해 감소! {damage} -> {reducedDamage}");
            return reducedDamage;
        }

        return damage;
    }

    private bool CanTakeDamage()
    {
        return !isDead && playerStat != null;
    }

    private void ApplyDamage(float damage)
    {
        playerStat.TakeDamage(damage);
        playerHealthBar?.UpdateHealthBar(playerStat.CurrentHp, playerStat.MaxHp);
        Debug.Log($"플레이어 피격! 남은 체력: {playerStat.CurrentHp}");
    }

    private bool IsDeadByHp()
    {
        return playerStat.CurrentHp <= 0f;
    }

    private void PlayHitReaction()
    {
        // 피격 애니메이션 / 넉백 / 이펙트
    }

    private void Die()
    {
        if (isDead)
            return;

        isDead = true;

        playerStat.SetCurrentHp(0f);

        playerAttack?.ResetAttackState();
        playerShield?.ResetShieldState();

        playerAnimation?.PlayDie();
        playerActionLock?.OnDie();

        Debug.Log("플레이어 사망");

        StartCoroutine(RespawnRoutine());
    }

    private IEnumerator RespawnRoutine()
    {
        yield return new WaitForSeconds(respawnDelay);
        Respawn();
    }

    private void Respawn()
    {
        if (playerStat == null)
            return;

        isDead = false;

        if (characterController != null)
            characterController.enabled = false;

        transform.position = respawnPosition;

        if (characterController != null)
            characterController.enabled = true;

        playerStat.SetCurrentHp(playerStat.MaxHp);
        playerHealthBar?.UpdateHealthBar(playerStat.CurrentHp, playerStat.MaxHp);

        playerAnimation?.ResetAnimation();
        playerAttack?.ResetAttackState();
        playerShield?.ResetShieldState();
        playerActionLock?.ResetState();

        Debug.Log("플레이어 부활");
    }

    public void Heal(float amount)
    {
        if (isDead || playerStat == null)
            return;

        playerStat.Heal(amount);
        Debug.Log($"플레이어 회복! 현재 체력: {playerStat.CurrentHp}");
    }

    public void Revive()
    {
        if (playerStat == null)
            return;

        StopAllCoroutines();
        Respawn();
    }

    public void SetRespawnPosition(Vector3 position)
    {
        respawnPosition = position;
    }
}