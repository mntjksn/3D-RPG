using System.Collections;
using UnityEngine;
using Photon.Pun;

// 플레이어 HP 처리, 사망/부활, 포션 사용 담당
public class PlayerHealth : MonoBehaviourPun, IDamageable
{
    private PlayerStat playerStat;
    private PlayerAnimation playerAnimation;
    private PlayerActionLock playerActionLock;
    private CharacterController characterController;
    private PlayerAttack playerAttack;
    private PlayerShield playerShield;
    private PlayerHealthBar playerHealthBar;
    private HitFlash hitFlash;

    private bool isDead;
    private Vector3 respawnPosition;

    [Header("Auto Heal")]
    [SerializeField] private float healDelay = 3f; // 피격 후 자동 회복 시작까지 대기 시간
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
        hitFlash = GetComponent<HitFlash>();
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

    // 자동 HP 재생 - 피격 후 healDelay 초 이후, HP가 가득 차지 않았을 때만 동작
    private void Update()
    {
        if (!photonView.IsMine) return;
        if (isDead || playerStat == null) return;
        if (Time.time < lastHitTime + healDelay) return;
        if (playerStat.CurrentHp >= playerStat.MaxHp) return;

        float healAmount = playerStat.MaxHp * playerStat.GetRegen() * Time.deltaTime;
        playerStat.Heal(healAmount);
    }

    // HP 변경 시 UI 갱신 및 다른 클라이언트에 동기화
    private void HandleHpChanged(float currentHp, float maxHp)
    {
        playerHealthBar?.UpdateHealthBar(currentHp, maxHp);

        if (photonView.IsMine)
            photonView.RPC(nameof(RPC_SyncHealthBar), RpcTarget.Others, currentHp, maxHp, isDead);
    }

    // 외부(PlayerSpawner)에서 HP바 컴포넌트를 연결할 때 호출
    public void SetHealthBar(PlayerHealthBar healthBar)
    {
        playerHealthBar = healthBar;

        if (playerStat != null && playerHealthBar != null)
            playerHealthBar.UpdateHealthBar(playerStat.CurrentHp, playerStat.MaxHp);
    }

    // 로컬 직접 피격 (트리거 충돌 등 로컬 호출용)
    public void TakeDamage(float damage)
    {
        if (!photonView.IsMine) return;
        if (!CanTakeDamage()) return;

        ApplyDamage(damage);

        if (IsDeadByHp())
            Die();
    }

    // 네트워크를 통해 피격 처리 (다른 클라이언트가 호출)
    [PunRPC]
    public void RPC_TakeDamage(float damage)
    {
        if (!photonView.IsMine) return;
        if (!CanTakeDamage()) return;

        ApplyDamage(damage);

        if (IsDeadByHp())
            Die();
    }

    // 원격 클라이언트의 HP바 동기화
    [PunRPC]
    private void RPC_SyncHealthBar(float currentHp, float maxHp, bool dead)
    {
        if (photonView.IsMine) return;

        isDead = dead;
        playerHealthBar?.UpdateHealthBar(currentHp, maxHp);
    }

    // 방어 중이고 공격자가 방어 범위 안에 있으면 방어력만큼 피해 감소
    public float ModifyIncomingDamage(Transform attacker, float damage)
    {
        if (isDead) return 0f;

        if (playerShield != null && playerShield.CanBlock(attacker))
        {
            float shieldPower = playerStat.GetShieldPower();
            float reducedDamage = damage * (1f - shieldPower / 100f);
            SoundManager.Instance?.PlaySFX(SfxType.PlayerShield);
            return reducedDamage;
        }

        return damage;
    }

    private bool CanTakeDamage()
    {
        return !isDead && playerStat != null;
    }

    // 피해 적용 - 히트 이펙트 재생 및 마지막 피격 시간 갱신
    private void ApplyDamage(float damage)
    {
        playerStat.TakeDamage(damage);
        hitFlash?.PlayFlash();
        lastHitTime = Time.time;
    }

    private bool IsDeadByHp()
    {
        return playerStat.CurrentHp <= 0f;
    }

    // 사망 처리 - 골드 패널티, 상태 잠금, 전체 클라에 사망 연출 동기화
    private void Die()
    {
        if (isDead) return;

        isDead = true;
        playerStat.SetCurrentHp(0f);

        // 골드의 25% 또는 최소 10골드를 패널티로 차감
        int currentGold = playerStat.Gold;
        int penalty = Mathf.Clamp(Mathf.Max(10, Mathf.FloorToInt(currentGold * 0.25f)), 0, currentGold);
        playerStat.UseGold(penalty);

        if (photonView.IsMine)
        {
            playerAttack?.ResetAttackState();
            playerShield?.ResetShieldState();
            playerActionLock?.OnDie();
        }

        photonView.RPC(nameof(RPC_PlayDie), RpcTarget.All);
        StartCoroutine(RespawnRoutine());
    }

    // 전체 클라이언트에서 사망 애니메이션 재생
    [PunRPC]
    private void RPC_PlayDie()
    {
        isDead = true;
        playerAnimation?.PlayDie();
    }

    private IEnumerator RespawnRoutine()
    {
        yield return new WaitForSeconds(respawnDelay);
        Respawn();
    }

    // 부활 처리 - 위치 초기화, HP 전체 회복, 전체 클라에 시각 상태 복구
    private void Respawn()
    {
        if (playerStat == null) return;

        isDead = false;

        // CharacterController 비활성화 후 이동 (활성 상태에서 위치 변경 시 충돌 오작동 방지)
        if (characterController != null)
            characterController.enabled = false;

        transform.position = respawnPosition;

        if (characterController != null)
            characterController.enabled = true;

        lastHitTime = Time.time;
        playerStat.SetCurrentHp(playerStat.MaxHp);

        if (photonView.IsMine)
        {
            playerAttack?.ResetAttackState();
            playerShield?.ResetShieldState();
            playerActionLock?.ResetState();
        }

        photonView.RPC(nameof(RPC_RespawnVisual), RpcTarget.All);
    }

    // 전체 클라이언트에서 애니메이션을 Idle로 복구
    [PunRPC]
    private void RPC_RespawnVisual()
    {
        isDead = false;
        playerAnimation?.ResetAnimation();
    }

    // 포션 사용 시도 - 아이템 유효성 및 수량 확인 후 효과 적용
    public bool TryUsePotion(ItemData itemData)
    {
        if (!photonView.IsMine) return false;
        if (isDead || itemData == null || playerStat == null) return false;
        if (playerStat.CurrentHp >= playerStat.MaxHp) return false;
        if (itemData.itemType != ItemType.Consumable) return false;
        if (InventoryManager.Instance == null) return false;
        if (InventoryManager.Instance.GetItemCount(itemData.itemId) <= 0) return false;

        bool removed = InventoryManager.Instance.RemoveItem(itemData.itemId, 1);
        if (!removed) return false;

        bool used = ApplyPotionEffect(itemData);
        if (!used)
        {
            // 효과 적용 실패 시 아이템 반환
            InventoryManager.Instance.AddItem(itemData, 1);
            return false;
        }

        SoundManager.Instance?.PlaySFX(SfxType.PotionUse);
        return true;
    }

    // 포션 ID에 따라 MaxHp 비율로 회복량 결정
    private bool ApplyPotionEffect(ItemData itemData)
    {
        switch (itemData.itemId)
        {
            case "potion_hp_small": playerStat.Heal(playerStat.MaxHp * 0.1f); return true;
            case "potion_hp_medium": playerStat.Heal(playerStat.MaxHp * 0.3f); return true;
            case "potion_hp_large": playerStat.Heal(playerStat.MaxHp * 0.5f); return true;
            case "potion_hp_full": playerStat.Heal(playerStat.MaxHp); return true;
        }

        Debug.LogWarning($"정의되지 않은 포션입니다: {itemData.itemId}");
        return false;
    }
}