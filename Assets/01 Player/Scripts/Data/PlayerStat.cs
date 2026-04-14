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

    public void InitializeStat()
    {
        if (playerData == null)
        {
            Debug.LogWarning("PlayerData가 비어 있습니다.");
            return;
        }

        level = playerData.startLevel;
        currentExp = 0;
        currentHp = GetMaxHp();
        lastMaxHp = GetMaxHp();
        gold = 0;

        NotifyAll();
    }

    public void SetCurrentHp(float value)
    {
        float newHp = Mathf.Clamp(value, 0f, GetMaxHp());

        if (Mathf.Approximately(currentHp, newHp))
            return;

        currentHp = newHp;
        OnHpChanged?.Invoke(currentHp, GetMaxHp());

        if (SaveManager.Instance != null)
            SaveManager.Instance.MarkDirty();
    }

    public void Heal(float amount)
    {
        if (amount <= 0f)
            return;

        SetCurrentHp(currentHp + amount);
    }

    public void TakeDamage(float damage)
    {
        if (damage <= 0f)
            return;

        SetCurrentHp(currentHp - damage);
    }

    public void AddGold(int amount)
    {
        if (playerData == null || amount <= 0)
            return;

        gold += amount;
        OnGoldChanged?.Invoke(gold);

        if (SaveManager.Instance != null)
            SaveManager.Instance.MarkDirty();
    }

    public bool UseGold(int amount)
    {
        if (amount <= 0 || gold < amount)
            return false;

        gold -= amount;
        OnGoldChanged?.Invoke(gold);

        if (SaveManager.Instance != null)
            SaveManager.Instance.MarkDirty();

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
    }

    public int GetExpToNextLevel()
    {
        if (playerData == null)
            return 0;

        return playerData.expToLevelUp * level;
    }

    public float GetMaxHp()
    {
        if (playerData == null)
            return 0f;

        float totalHp = playerData.maxHp;

        if (EquipmentManager.Instance != null)
        {
            ItemData armor = EquipmentManager.Instance.GetEquippedItem(EquipmentSlotType.Armor);

            if (armor != null)
                totalHp += armor.maxHpBonus;
        }

        if (UpgradeManager.Instance != null)
        {
            int HpLevel = UpgradeManager.Instance.GetCurrentLevel(UpgradeType.Hp);
            totalHp += HpLevel * 50;
        }

        return totalHp;
    }

    public float GetAttackPower()
    {
        if (playerData == null)
            return 0f;

        float totalAttack = playerData.attackPower;

        if (EquipmentManager.Instance != null)
        {
            ItemData weapon = EquipmentManager.Instance.GetEquippedItem(EquipmentSlotType.Weapon);

            if (weapon != null)
                totalAttack += weapon.attackPower;
        }

        if (UpgradeManager.Instance != null)
        {
            int attackLevel = UpgradeManager.Instance.GetCurrentLevel(UpgradeType.Attack);
            totalAttack += attackLevel * 5;
        }

        return totalAttack;
    }

    public float GetShieldPower()
    {
        if (playerData == null)
            return 0f;

        float totalDefense = playerData.shieldPower;

        if (EquipmentManager.Instance != null)
        {
            ItemData shield = EquipmentManager.Instance.GetEquippedItem(EquipmentSlotType.Shield);

            if (shield != null)
                totalDefense += shield.shieldPower;
        }

        return totalDefense;
    }

    public float GetSpeed()
    {
        if (playerData == null)
            return 0f;

        float totalSpeed = playerData.speed;

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

        float totaRegen = playerData.regen;

        if (UpgradeManager.Instance != null)
        {
            float regenLevel = UpgradeManager.Instance.GetCurrentLevel(UpgradeType.Regen);
            totaRegen += regenLevel * 0.01f;
        }

        return totaRegen;
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
        currentHp = Mathf.Clamp(saveData.currentHp, 0f, GetMaxHp());
        lastMaxHp = GetMaxHp();
        gold = Mathf.Max(0, saveData.gold);

        NotifyAll();
    }

    private void HandleEquipmentChanged()
    {
        float newMaxHp = GetMaxHp();
        float delta = newMaxHp - lastMaxHp;

        if (!Mathf.Approximately(delta, 0f))
            currentHp = Mathf.Clamp(currentHp + delta, 0f, newMaxHp);
        else
            currentHp = Mathf.Clamp(currentHp, 0f, newMaxHp);

        lastMaxHp = newMaxHp;

        OnHpChanged?.Invoke(currentHp, newMaxHp);
    }

    private bool CanLevelUp()
    {
        return currentExp >= GetExpToNextLevel();
    }

    private void LevelUp()
    {
        currentExp -= GetExpToNextLevel();
        level++;
        currentHp = GetMaxHp();
        lastMaxHp = GetMaxHp();

        Debug.Log($"레벨업! 현재 레벨: {level}");

        OnLevelChanged?.Invoke(level);
        OnHpChanged?.Invoke(currentHp, GetMaxHp());
    }

    private void NotifyAll()
    {
        OnLevelChanged?.Invoke(level);
        OnExpChanged?.Invoke(currentExp, GetExpToNextLevel());
        OnHpChanged?.Invoke(currentHp, GetMaxHp());
        OnGoldChanged?.Invoke(gold);
    }
}