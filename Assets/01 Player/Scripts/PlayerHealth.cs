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

    [Header("Auto Heal")]
    [SerializeField] private float healDelay = 3f;      // 몇 초 후 시작
    [SerializeField] private float healPerSecond = 5f;  // 초당 회복량

    private float lastHitTime;

    [Header("Respawn Delay")]
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

    private void OnEnable()
    {
        if (playerStat != null)
            playerStat.OnHpChanged += HandleHpChanged;
    }

    private void OnDisable()
    {
        if (playerStat != null)
            playerStat.OnHpChanged -= HandleHpChanged;
    }

    private void Update()
    {
        if (isDead || playerStat == null)
            return;

        // 아직 대기 시간 안 지났으면 회복 안함
        if (Time.time < lastHitTime + healDelay)
            return;

        // 이미 풀피면 회복 안함
        if (playerStat.CurrentHp >= playerStat.MaxHp)
            return;

        float healAmount = healPerSecond * Time.deltaTime;
        playerStat.Heal(healAmount);
    }

    private void HandleHpChanged(float currentHp, float maxHp)
    {
        playerHealthBar?.UpdateHealthBar(currentHp, maxHp);
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
        lastHitTime = Time.time;
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