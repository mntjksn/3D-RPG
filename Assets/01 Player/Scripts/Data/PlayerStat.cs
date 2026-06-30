using System;
using UnityEngine;

// 플레이어 스탯 계산 및 상태 관리 (HP, EXP, 골드, 레벨)
public class PlayerStat : MonoBehaviour
{
    [Header("Data")]
    [SerializeField] private PlayerData playerData;

    // 스탯 변경 시 UI 등 외부에서 구독할 이벤트
    public event Action<int> OnLevelChanged;
    public event Action<int, int> OnExpChanged;
    public event Action<float, float> OnHpChanged;
    public event Action<int> OnGoldChanged;

    private int level;
    private int currentExp;
    private float currentHp;
    private int gold;

    private float lastMaxHp; // 장비 변경 시 HP 보정을 위한 이전 최대 HP

    // 읽기 전용 프로퍼티
    public int Level => level;
    public int CurrentExp => currentExp;
    public float CurrentHp => currentHp;
    public int Gold => gold;

    // 장비/업그레이드 포함 계산 결과 프로퍼티
    public float MaxHp => GetMaxHp();
    public float AttackPower => GetAttackPower();
    public float ShieldPower => GetShieldPower();
    public float Speed => GetSpeed();
    public float Regen => GetRegen();

    private void Awake()
    {
        InitializeStat();
    }

    private void OnEnable()
    {
        if (EquipmentManager.Instance != null)
            EquipmentManager.Instance.OnEquipmentChanged += HandleEquipmentChanged;
    }

    private void OnDisable()
    {
        if (EquipmentManager.Instance != null)
            EquipmentManager.Instance.OnEquipmentChanged -= HandleEquipmentChanged;
    }

    // 강제로 모든 이벤트를 다시 발행 (로드 후 UI 갱신 등에 사용)
    public void ForceNotify()
    {
        NotifyAll();
    }

    // 스탯 초기화 - 게임 시작 또는 세이브 없을 때 호출
    public void InitializeStat()
    {
        if (playerData == null)
        {
            Debug.LogWarning("PlayerData가 비어 있습니다.");
            return;
        }

        level = playerData.startLevel;
        currentExp = 0;
        gold = 0;

        float maxHp = GetMaxHp();
        currentHp = maxHp;
        lastMaxHp = maxHp;

        NotifyAll();
    }

    // HP를 직접 설정 (0 ~ MaxHp 사이로 클램프)
    public void SetCurrentHp(float value)
    {
        float maxHp = GetMaxHp();
        float newHp = Mathf.Clamp(value, 0f, maxHp);

        if (Mathf.Approximately(currentHp, newHp))
            return;

        currentHp = newHp;
        OnHpChanged?.Invoke(currentHp, maxHp);
        MarkDirty();
    }

    public void Heal(float amount)
    {
        if (amount <= 0f) return;
        SetCurrentHp(currentHp + amount);
    }

    public void TakeDamage(float damage)
    {
        if (damage <= 0f) return;
        SetCurrentHp(currentHp - damage);
    }

    public void AddGold(int amount)
    {
        if (playerData == null || amount <= 0) return;

        gold += amount;
        OnGoldChanged?.Invoke(gold);
        MarkDirty();
    }

    // 골드 차감 - 잔액 부족 시 false 반환
    public bool UseGold(int amount)
    {
        if (amount <= 0 || gold < amount)
            return false;

        gold -= amount;
        OnGoldChanged?.Invoke(gold);
        MarkDirty();
        return true;
    }

    // 경험치 추가 및 레벨업 처리
    public void AddExp(int amount)
    {
        if (playerData == null || amount <= 0) return;

        currentExp += amount;

        while (CanLevelUp())
            LevelUp();

        OnExpChanged?.Invoke(currentExp, GetExpToNextLevel());
        MarkDirty();
    }

    // 다음 레벨까지 필요한 경험치
    public int GetExpToNextLevel()
    {
        return playerData != null ? playerData.expToLevelUp * level : 0;
    }

    // 기본값 + 장비 보너스 + 업그레이드 보너스를 합산한 최대 HP
    public int GetMaxHp()
    {
        if (playerData == null) return 0;

        int total = playerData.maxHp;

        ItemData armor = EquipmentManager.Instance?.GetEquippedItem(EquipmentSlotType.Armor);
        if (armor != null)
            total += armor.maxHpBonus;

        if (UpgradeManager.Instance != null)
            total += (UpgradeManager.Instance.GetCurrentLevel(UpgradeType.Hp) - 1) * 50;

        return total;
    }

    // 기본값 + 무기 보너스 + 업그레이드 보너스를 합산한 공격력
    public int GetAttackPower()
    {
        if (playerData == null) return 0;

        int total = playerData.attackPower;

        ItemData weapon = EquipmentManager.Instance?.GetEquippedItem(EquipmentSlotType.Weapon);
        if (weapon != null)
            total += weapon.attackPower;

        if (UpgradeManager.Instance != null)
            total += (UpgradeManager.Instance.GetCurrentLevel(UpgradeType.Attack) - 1) * 5;

        return total;
    }

    // 기본값 + 방패 보너스를 합산한 방어력
    public int GetShieldPower()
    {
        if (playerData == null) return 0;

        int total = playerData.shieldPower;

        ItemData shield = EquipmentManager.Instance?.GetEquippedItem(EquipmentSlotType.Shield);
        if (shield != null)
            total += shield.shieldPower;

        return total;
    }

    // 기본값 + 신발 보너스를 합산한 이동 속도
    public int GetSpeed()
    {
        if (playerData == null) return 0;

        int total = playerData.speed;

        ItemData shoes = EquipmentManager.Instance?.GetEquippedItem(EquipmentSlotType.Shoes);
        if (shoes != null)
            total += shoes.moveSpeedBonus;

        return total;
    }

    // 기본값 + 업그레이드 보너스를 합산한 HP 재생량
    public float GetRegen()
    {
        if (playerData == null) return 0f;

        float total = playerData.regen;

        if (UpgradeManager.Instance != null)
            total += (UpgradeManager.Instance.GetCurrentLevel(UpgradeType.Regen) - 1) * 0.01f;

        return total;
    }

    // 현재 스탯을 세이브 데이터 구조체로 반환
    public PlayerSaveData GetSaveData()
    {
        return new PlayerSaveData
        {
            level = level,
            currentExp = currentExp,
            currentHp = currentHp,
            gold = gold
        };
    }

    // 세이브 데이터를 스탯에 적용 (최솟값 보정 포함)
    public void LoadFromSaveData(PlayerSaveData saveData)
    {
        if (saveData == null || playerData == null) return;

        level = Mathf.Max(playerData.startLevel, saveData.level);
        currentExp = Mathf.Max(0, saveData.currentExp);
        gold = Mathf.Max(0, saveData.gold);

        float maxHp = GetMaxHp();
        currentHp = Mathf.Clamp(saveData.currentHp, 0f, maxHp);
        lastMaxHp = maxHp;

        NotifyAll();
    }

    // 장비 변경 시 최대 HP 증감분만큼 현재 HP도 조정
    private void HandleEquipmentChanged()
    {
        float newMaxHp = GetMaxHp();
        float delta = newMaxHp - lastMaxHp;

        currentHp = Mathf.Clamp(currentHp + delta, 0f, newMaxHp);
        lastMaxHp = newMaxHp;

        OnHpChanged?.Invoke(currentHp, newMaxHp);
        MarkDirty();
    }

    private bool CanLevelUp()
    {
        return currentExp >= GetExpToNextLevel();
    }

    // 레벨업 처리 - EXP 차감, 레벨 증가, HP 전체 회복
    private void LevelUp()
    {
        currentExp -= GetExpToNextLevel();
        level++;

        float maxHp = GetMaxHp();
        currentHp = maxHp;
        lastMaxHp = maxHp;

        OnLevelChanged?.Invoke(level);
        OnHpChanged?.Invoke(currentHp, maxHp);
    }

    // 모든 스탯 이벤트를 일괄 발행
    private void NotifyAll()
    {
        float maxHp = GetMaxHp();

        OnLevelChanged?.Invoke(level);
        OnExpChanged?.Invoke(currentExp, GetExpToNextLevel());
        OnHpChanged?.Invoke(currentHp, maxHp);
        OnGoldChanged?.Invoke(gold);
    }

    private void MarkDirty()
    {
        SaveManager.Instance?.MarkDirty();
    }
}