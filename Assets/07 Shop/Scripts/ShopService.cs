using UnityEngine;

// 상점 구매, 판매 처리 담당
public static class ShopService
{
    // 최대 구매 가능 수량 계산
    public static int GetMaxBuyCount(ItemData itemData, PlayerStat playerStat)
    {
        if (itemData == null || playerStat == null)
            return 1;

        if (itemData.buyPrice <= 0)
            return 999;

        int maxByGold = playerStat.Gold / itemData.buyPrice;
        return Mathf.Max(1, maxByGold);
    }

    // 아이템 구매 처리
    public static bool TryBuy(
        int shopSlotIndex,
        int quantity,
        ShopUI shopUI,
        PlayerStat playerStat)
    {
        if (shopUI == null) return false;
        if (playerStat == null) return false;
        if (InventoryManager.Instance == null) return false;
        if (quantity <= 0) return false;

        ItemData itemData = shopUI.GetItemDataByShopIndex(shopSlotIndex);
        if (itemData == null) return false;

        int totalPrice = itemData.buyPrice * quantity;
        if (playerStat.Gold < totalPrice) return false;

        if (!InventoryManager.Instance.AddItem(itemData, quantity))
            return false;

        if (!playerStat.UseGold(totalPrice))
        {
            InventoryManager.Instance.RemoveItem(itemData.itemId, quantity);
            return false;
        }

        ShopManager.Instance?.NotifyTradeSuccess();
        QuestService.NotifyBuyItem(itemData.itemName, quantity);
        SoundManager.Instance?.PlaySFX(SfxType.BuySell);
        return true;
    }

    // 아이템 판매 처리
    public static bool TrySell(
        int inventorySlotIndex,
        int quantity,
        PlayerStat playerStat)
    {
        if (playerStat == null) return false;
        if (InventoryManager.Instance == null) return false;
        if (quantity <= 0) return false;

        ItemData itemData = InventoryManager.Instance.GetItemAtSlot(inventorySlotIndex);
        if (itemData == null) return false;

        int ownedCount = InventoryManager.Instance.GetItemCountAtSlot(inventorySlotIndex);
        if (ownedCount < quantity) return false;

        if (!InventoryManager.Instance.RemoveItemAtSlot(inventorySlotIndex, quantity))
            return false;

        int totalSellPrice = itemData.sellPrice * quantity;
        playerStat.AddGold(totalSellPrice);

        ShopManager.Instance?.NotifyTradeSuccess();
        QuestService.NotifySellItem(itemData.itemName, quantity);
        SoundManager.Instance?.PlaySFX(SfxType.BuySell);
        return true;
    }
}