using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "UpgradeTable", menuName = "Upgrade/Upgrade Table")]
// 업그레이드 단계별 데이터 테이블
public class UpgradeTable : ScriptableObject
{
    [Tooltip("1->2, 2->3, 3->4 ... 순서대로 넣기")]
    public List<UpgradeLevelData> levels = new();

    // 다음 레벨 업그레이드 데이터 반환
    public UpgradeLevelData GetLevelData(int currentLevel, int maxLevel = 10)
    {
        if (currentLevel >= maxLevel)
            return null;

        int index = currentLevel - 1; // 1레벨 → 0번

        if (index < 0 || index >= levels.Count)
            return null;

        return levels[index];
    }
}