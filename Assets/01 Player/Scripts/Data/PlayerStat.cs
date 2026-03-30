using UnityEngine;

public class PlayerStat : MonoBehaviour
{
    [Header("Data")]
    [SerializeField] private PlayerData playerData;

    private int level;
    private int currentExp;
    private float currentHp;

    public int Level => level;
    public int CurrentExp => currentExp;
    public float CurrentHp => currentHp;

    public float MaxHp => GetMaxHp();
    public float AttackPower => GetAttackPower();

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
    }

    public void SetCurrentHp(float value)
    {
        currentHp = Mathf.Clamp(value, 0f, GetMaxHp());
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

    public void AddExp(int amount)
    {
        if (playerData == null || amount <= 0)
            return;

        currentExp += amount;

        while (CanLevelUp())
            LevelUp();
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

        return playerData.maxHp + (level - playerData.startLevel) * playerData.hpPerLevel;
    }

    public float GetAttackPower()
    {
        if (playerData == null)
            return 0f;

        return playerData.attackPower + (level - playerData.startLevel) * playerData.attackPerLevel;
    }

    public PlayerSaveData GetSaveData()
    {
        return new PlayerSaveData
        {
            level = level,
            currentExp = currentExp,
            currentHp = currentHp
        };
    }

    public void LoadFromSaveData(PlayerSaveData saveData)
    {
        if (saveData == null || playerData == null)
            return;

        level = Mathf.Max(playerData.startLevel, saveData.level);
        currentExp = Mathf.Max(0, saveData.currentExp);
        currentHp = Mathf.Clamp(saveData.currentHp, 0f, GetMaxHp());
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
    }
}