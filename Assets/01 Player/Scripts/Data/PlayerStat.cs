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

    public int Level => level;
    public int CurrentExp => currentExp;
    public float CurrentHp => currentHp;
    public int Gold => gold;

    public float MaxHp => GetMaxHp();
    public float AttackPower => GetAttackPower();
    public float ShieldPower => GetShieldPower();
    public float Speed => GetSpeed();

    private void Awake()
    {
        InitializeStat();
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
    }

    public bool UseGold(int amount)
    {
        if (amount <= 0 || gold < amount)
            return false;

        gold -= amount;
        OnGoldChanged?.Invoke(gold);
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

        return playerData.expToLevelUp;
    }

    public float GetMaxHp()
    {
        if (playerData == null)
            return 0f;

        return playerData.maxHp;
    }

    public float GetAttackPower()
    {
        if (playerData == null)
            return 0f;

        return playerData.attackPower;
    }

    public float GetShieldPower()
    {
        if (playerData == null)
            return 0f;

        return playerData.shieldPower;
    }

    public float GetSpeed()
    {
        if (playerData == null)
            return 0f;

        return playerData.speed;
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
        currentHp = Mathf.Max(0, GetMaxHp());
        gold = Mathf.Max(0, saveData.gold);
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