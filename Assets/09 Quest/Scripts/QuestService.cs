using UnityEngine;

public static class QuestService
{
    public static void AddProgress(QuestData questData, int amount)
    {
        if (questData == null || amount <= 0)
            return;

        QuestManager manager = QuestManager.Instance;
        if (manager == null)
            return;

        QuestStateData state = manager.GetState(questData.questId);
        if (state == null || !state.isAccepted || state.isCompleted)
            return;

        int newValue = state.currentCount + amount;
        manager.SetProgress(questData, newValue);
    }

    public static void NotifyKill(string enemyId)
    {
        QuestManager manager = QuestManager.Instance;
        if (manager == null || string.IsNullOrEmpty(enemyId))
            return;

        foreach (QuestData questData in manager.GetActiveQuestDatas())
        {
            if (questData == null)
                continue;

            if (questData.questType != QuestType.KillMonster)
                continue;

            if (!string.Equals(questData.targetId, enemyId))
                continue;

            AddProgress(questData, 1);
        }
    }

    public static void NotifyCollectItem(string itemId, int amount)
    {
        QuestManager manager = QuestManager.Instance;
        if (manager == null || string.IsNullOrEmpty(itemId) || amount <= 0)
            return;

        foreach (QuestData questData in manager.GetActiveQuestDatas())
        {
            if (questData == null)
                continue;

            if (questData.questType != QuestType.CollectItem)
                continue;

            if (!string.Equals(questData.targetId, itemId))
                continue;

            AddProgress(questData, amount);
        }
    }

    public static void NotifyBuyItem(string itemId, int amount)
    {
        QuestManager manager = QuestManager.Instance;
        if (manager == null || string.IsNullOrEmpty(itemId) || amount <= 0)
            return;

        foreach (QuestData questData in manager.GetActiveQuestDatas())
        {
            if (questData == null)
                continue;

            if (questData.questType != QuestType.BuyItem)
                continue;

            if (!string.Equals(questData.targetId, itemId))
                continue;

            AddProgress(questData, amount);
        }
    }

    public static void NotifySellItem(string itemId, int amount)
    {
        QuestManager manager = QuestManager.Instance;
        if (manager == null || string.IsNullOrEmpty(itemId) || amount <= 0)
            return;

        foreach (QuestData questData in manager.GetActiveQuestDatas())
        {
            if (questData == null)
                continue;

            if (questData.questType != QuestType.SellItem)
                continue;

            if (!string.Equals(questData.targetId, itemId))
                continue;

            AddProgress(questData, amount);
        }
    }

    public static void NotifyEquipItem(string itemId)
    {
        QuestManager manager = QuestManager.Instance;
        if (manager == null || string.IsNullOrEmpty(itemId))
            return;

        foreach (QuestData questData in manager.GetActiveQuestDatas())
        {
            if (questData == null)
                continue;

            if (questData.questType != QuestType.EquipItem)
                continue;

            if (!string.Equals(questData.targetId, itemId))
                continue;

            AddProgress(questData, 1);
        }
    }

    public static void NotifyUpgrade(string targetId)
    {
        QuestManager manager = QuestManager.Instance;
        if (manager == null || string.IsNullOrEmpty(targetId))
            return;

        foreach (QuestData questData in manager.GetActiveQuestDatas())
        {
            if (questData == null)
                continue;

            if (questData.questType != QuestType.Upgrade)
                continue;

            if (!string.Equals(questData.targetId, targetId))
                continue;

            AddProgress(questData, 1);
        }
    }

    public static bool CanClaimReward(QuestData questData)
    {
        if (questData == null || QuestManager.Instance == null)
            return false;

        QuestStateData state = QuestManager.Instance.GetState(questData.questId);
        if (state == null)
            return false;

        return state.isAccepted && state.isCompleted && !state.isRewardClaimed;
    }

    public static bool ClaimReward(QuestData questData)
    {
        if (!CanClaimReward(questData))
            return false;

        if ((questData.rewardGold > 0 || questData.rewardExp > 0) && PlayerManager.Instance == null)
            return false;

        if (questData.rewardItem != null && questData.rewardItemCount > 0)
        {
            if (InventoryManager.Instance == null)
                return false;

            bool added = InventoryManager.Instance.AddItem(questData.rewardItem, questData.rewardItemCount);
            if (!added)
                return false;
        }

        if (questData.rewardGold > 0)
            PlayerManager.Instance.AddGold(questData.rewardGold);

        if (questData.rewardExp > 0)
            PlayerManager.Instance.AddExp(questData.rewardExp);

        QuestManager.Instance?.MarkRewardClaimed(questData);
        return true;
    }
}