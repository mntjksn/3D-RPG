using UnityEngine;

[CreateAssetMenu(fileName = "StatUpgradeData", menuName = "Upgrade/Stat Upgrade Data")]
public class StatUpgradeData : ScriptableObject
{
    [Header("Info")]
    public UpgradeType upgradeType;

    [Header("Value")]
    public float baseValue = 1f;
    public float valuePerLevel = 1f;
    public int maxLevel = 10;

    [Header("Table")]
    public UpgradeTable upgradeTable;

    public float GetValue(int level)
    {
        level = Mathf.Clamp(level, 1, maxLevel);
        return baseValue + (level - 1) * valuePerLevel;
    }

    public bool IsMaxLevel(int level)
    {
        return level >= maxLevel;
    }

    public UpgradeLevelData GetNextLevelData(int currentLevel)
    {
        if (upgradeTable == null)
            return null;

        return upgradeTable.GetLevelData(currentLevel, maxLevel);
    }
}