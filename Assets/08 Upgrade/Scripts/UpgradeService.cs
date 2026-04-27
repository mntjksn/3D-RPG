using UnityEngine;

// 업그레이드 가능 여부 검사 및 강화 처리 담당
public static class UpgradeService
{
    // 업그레이드 가능 여부 확인
    public static bool CanUpgrade(
        UpgradeManager manager,
        UpgradeType type,
        StatUpgradeData statData,
        PlayerStat playerStat,
        out UpgradeLevelData levelData)
    {
        levelData = null;

        if (manager == null || statData == null || playerStat == null || InventoryManager.Instance == null)
            return false;

        int currentLevel = manager.GetCurrentLevel(type);
        if (statData.IsMaxLevel(currentLevel))
            return false;

        levelData = statData.GetNextLevelData(currentLevel);
        if (levelData == null)
            return false;

        ItemData selectedMaterial = manager.GetSelectedMaterial(type);
        if (selectedMaterial == null || levelData.item == null)
            return false;

        if (selectedMaterial.itemId != levelData.item.itemId)
            return false;

        int ownedCount = InventoryManager.Instance.GetItemCount(selectedMaterial.itemId);
        if (ownedCount < levelData.amount)
            return false;

        if (playerStat.Gold < levelData.goldCost)
            return false;

        return true;
    }

    // 업그레이드 시도
    public static bool TryUpgrade(
        UpgradeManager manager,
        UpgradeType type,
        StatUpgradeData statData,
        PlayerStat playerStat)
    {
        if (!CanUpgrade(manager, type, statData, playerStat, out UpgradeLevelData levelData))
            return false;

        ItemData selectedMaterial = manager.GetSelectedMaterial(type);
        if (selectedMaterial == null)
            return false;

        if (!InventoryManager.Instance.RemoveItem(selectedMaterial.itemId, levelData.amount))
            return false;

        if (!playerStat.UseGold(levelData.goldCost))
        {
            InventoryManager.Instance.AddItem(selectedMaterial, levelData.amount);
            return false;
        }

        bool success = Random.value <= levelData.successChance;

        if (success)
        {
            manager.AddLevel(type);
            manager.OnUpgradeSuccess?.Invoke(type);

            SoundManager.Instance?.PlaySFX(SfxType.BuySell);
            QuestService.NotifyUpgrade(type.ToString());
        }

        SaveManager.Instance?.MarkDirty();
        return success;
    }
}