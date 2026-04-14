using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "UpgradeTable", menuName = "Upgrade/Upgrade Table")]
public class UpgradeTable : ScriptableObject
{
    [Tooltip("1->2, 2->3, 3->4 ... 순서대로 넣기")]
    public List<UpgradeLevelData> levels = new List<UpgradeLevelData>();

    public UpgradeLevelData GetLevelData(int currentLevel, int maxLevel = 10)
    {
        if (currentLevel >= maxLevel)
            return null;

        int index = currentLevel - 1; // level 1이면 0번 데이터 사용

        if (index < 0 || index >= levels.Count)
            return null;

        return levels[index];
    }
}