using UnityEngine;

// 퀘스트 진행 알림 및 보상 처리 담당
public static class QuestService
{
    // 진행도 증가
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

    // 몬스터 처치
    public static void NotifyKill(string enemyId)
    {
        QuestManager manager = QuestManager.Instance;
        if (manager == null || string.IsNullOrEmpty(enemyId))
            return;

        foreach (QuestData questData in manager.GetActiveQuestDatas())
        {
            if (questData == null) continue;
            if (questData.questType != QuestType.KillMonster) continue;
            if (!string.Equals(questData.targetId, enemyId)) continue;

            AddProgress(questData, 1);
        }
    }

    // 아이템 획득
    public static void NotifyCollectItem(string itemId, int amount)
    {
        QuestManager manager = QuestManager.Instance;
        if (manager == null || string.IsNullOrEmpty(itemId) || amount <= 0)
            return;

        foreach (QuestData questData in manager.GetActiveQuestDatas())
        {
            if (questData == null) continue;
            if (questData.questType != QuestType.CollectItem) continue;
            if (!string.Equals(questData.targetId, itemId)) continue;

            AddProgress(questData, amount);
        }
    }

    // 아이템 구매
    public static void NotifyBuyItem(string itemId, int amount)
    {
        QuestManager manager = QuestManager.Instance;
        if (manager == null || string.IsNullOrEmpty(itemId) || amount <= 0)
            return;

        foreach (QuestData questData in manager.GetActiveQuestDatas())
        {
            if (questData == null) continue;
            if (questData.questType != QuestType.BuyItem) continue;
            if (!string.Equals(questData.targetId, itemId)) continue;

            AddProgress(questData, amount);
        }
    }

    // 아이템 판매
    public static void NotifySellItem(string itemId, int amount)
    {
        QuestManager manager = QuestManager.Instance;
        if (manager == null || string.IsNullOrEmpty(itemId) || amount <= 0)
            return;

        foreach (QuestData questData in manager.GetActiveQuestDatas())
        {
            if (questData == null) continue;
            if (questData.questType != QuestType.SellItem) continue;
            if (!string.Equals(questData.targetId, itemId)) continue;

            AddProgress(questData, amount);
        }
    }

    // 아이템 장착
    public static void NotifyEquipItem(string itemId)
    {
        QuestManager manager = QuestManager.Instance;
        if (manager == null || string.IsNullOrEmpty(itemId))
            return;

        foreach (QuestData questData in manager.GetActiveQuestDatas())
        {
            if (questData == null) continue;
            if (questData.questType != QuestType.EquipItem) continue;
            if (!string.Equals(questData.targetId, itemId)) continue;

            AddProgress(questData, 1);
        }
    }

    // 업그레이드
    public static void NotifyUpgrade(string targetId)
    {
        QuestManager manager = QuestManager.Instance;
        if (manager == null || string.IsNullOrEmpty(targetId))
            return;

        foreach (QuestData questData in manager.GetActiveQuestDatas())
        {
            if (questData == null) continue;
            if (questData.questType != QuestType.Upgrade) continue;
            if (!string.Equals(questData.targetId, targetId)) continue;

            AddProgress(questData, 1);
        }
    }

    // 보상 수령 가능 여부
    public static bool CanClaimReward(QuestData questData)
    {
        if (questData == null || QuestManager.Instance == null)
            return false;

        QuestStateData state = QuestManager.Instance.GetState(questData.questId);
        if (state == null)
            return false;

        return state.isAccepted && state.isCompleted && !state.isRewardClaimed;
    }

    // 보상 수령 처리
    public static bool ClaimReward(QuestData questData)
    {
        if (!CanClaimReward(questData))
            return false;

        if ((questData.rewardGold > 0 || questData.rewardExp > 0) && PlayerManager.Instance == null)
            return false;

        // 아이템 지급
        if (questData.rewardItem != null && questData.rewardItemCount > 0)
        {
            if (InventoryManager.Instance == null)
                return false;

            if (!InventoryManager.Instance.AddItem(questData.rewardItem, questData.rewardItemCount))
                return false;
        }

        // 골드 지급
        if (questData.rewardGold > 0)
            PlayerManager.Instance.AddGold(questData.rewardGold);

        // 경험치 지급
        if (questData.rewardExp > 0)
            PlayerManager.Instance.AddExp(questData.rewardExp);

        QuestManager.Instance?.MarkRewardClaimed(questData);
        return true;
    }
}