using UnityEngine;

[CreateAssetMenu(fileName = "StatUpgradeData", menuName = "Upgrade/Stat Upgrade Data")]
// 스탯 업그레이드 기본값 및 단계 데이터
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

    // 현재 레벨 기준 값 계산
    public float GetValue(int level)
    {
        level = Mathf.Clamp(level, 1, maxLevel);
        return baseValue + (level - 1) * valuePerLevel;
    }

    // 최대 레벨 여부
    public bool IsMaxLevel(int level)
    {
        return level >= maxLevel;
    }

    // 다음 레벨 업그레이드 데이터 조회
    public UpgradeLevelData GetNextLevelData(int currentLevel)
    {
        if (upgradeTable == null)
            return null;

        return upgradeTable.GetLevelData(currentLevel, maxLevel);
    }
}