using System;
using UnityEngine;

public class PlayerStat : MonoBehaviour
{
    [Header("Data")]
    [SerializeField] private PlayerData playerData;

    public event Action<int> OnLevelChanged;
    public event Action<int, int> OnExpChanged;
    public event Action<float, float> OnHpChanged;
    public event Action<int> OnGoldChanged;

    private int level;
    private int currentExp;
    private float currentHp;
    private int gold;

    private float lastMaxHp;

    public int Level => level;
    public int CurrentExp => currentExp;
    public float CurrentHp => currentHp;
    public int Gold => gold;

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

    public void ForceNotify()
    {
        NotifyAll();
    }

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
        if (playerData == null || amount <= 0)
            return;

        gold += amount;
        OnGoldChanged?.Invoke(gold);
        MarkDirty();
    }

    public bool UseGold(int amount)
    {
        if (amount <= 0 || gold < amount)
            return false;

        gold -= amount;
        OnGoldChanged?.Invoke(gold);
        MarkDirty();

        return true;
    }

    public void AddExp(int amount)
    {
        if (playerData == null || amount <= 0)
            return;

        currentExp += amount;

        while (CanLevelUp())
            LevelUp();

        OnExpChanged?.Invoke(currentExp, GetExpToNextLevel());
        MarkDirty();
    }

    public int GetExpToNextLevel()
    {
        if (playerData == null)
            return 0;

        return playerData.expToLevelUp * level;
    }

    public int GetMaxHp()
    {
        if (playerData == null)
            return 0;

        int totalHp = playerData.maxHp;

        if (EquipmentManager.Instance != null)
        {
            ItemData armor = EquipmentManager.Instance.GetEquippedItem(EquipmentSlotType.Armor);
            if (armor != null)
                totalHp += armor.maxHpBonus;
        }

        if (UpgradeManager.Instance != null)
        {
            int hpLevel = UpgradeManager.Instance.GetCurrentLevel(UpgradeType.Hp) - 1;
            totalHp += hpLevel * 50;
        }

        return totalHp;
    }

    public int GetAttackPower()
    {
        if (playerData == null)
            return 0;

        int totalAttack = playerData.attackPower;

        if (EquipmentManager.Instance != null)
        {
            ItemData weapon = EquipmentManager.Instance.GetEquippedItem(EquipmentSlotType.Weapon);
            if (weapon != null)
                totalAttack += weapon.attackPower;
        }

        if (UpgradeManager.Instance != null)
        {
            int level = UpgradeManager.Instance.GetCurrentLevel(UpgradeType.Attack) - 1;
            totalAttack += level * 5;
        }

        return totalAttack;
    }

    public int GetShieldPower()
    {
        if (playerData == null)
            return 0;

        int totalDefense = playerData.shieldPower;

        if (EquipmentManager.Instance != null)
        {
            ItemData shield = EquipmentManager.Instance.GetEquippedItem(EquipmentSlotType.Shield);
            if (shield != null)
                totalDefense += shield.shieldPower;
        }

        return totalDefense;
    }

    public int GetSpeed()
    {
        if (playerData == null)
            return 0;

        int totalSpeed = playerData.speed;

        if (EquipmentManager.Instance != null)
        {
            ItemData shoes = EquipmentManager.Instance.GetEquippedItem(EquipmentSlotType.Shoes);
            if (shoes != null)
                totalSpeed += shoes.moveSpeedBonus;
        }

        return totalSpeed;
    }

    public float GetRegen()
    {
        if (playerData == null)
            return 0f;

        float totalRegen = playerData.regen;

        if (UpgradeManager.Instance != null)
        {
            int level = UpgradeManager.Instance.GetCurrentLevel(UpgradeType.Regen) - 1;
            totalRegen += level * 0.01f;
        }

        return totalRegen;
    }

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

    public void LoadFromSaveData(PlayerSaveData saveData)
    {
        if (saveData == null || playerData == null)
            return;

        level = Mathf.Max(playerData.startLevel, saveData.level);
        currentExp = Mathf.Max(0, saveData.currentExp);
        gold = Mathf.Max(0, saveData.gold);

        float maxHp = GetMaxHp();
        currentHp = Mathf.Clamp(saveData.currentHp, 0f, maxHp);
        lastMaxHp = maxHp;

        NotifyAll();
    }

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

    private void LevelUp()
    {
        currentExp -= GetExpToNextLevel();
        level++;

        float maxHp = GetMaxHp();
        currentHp = maxHp;
        lastMaxHp = maxHp;

        Debug.Log($"레벨업! 현재 레벨: {level}");

        OnLevelChanged?.Invoke(level);
        OnHpChanged?.Invoke(currentHp, maxHp);
    }

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
        if (SaveManager.Instance != null)
            SaveManager.Instance.MarkDirty();
    }
}