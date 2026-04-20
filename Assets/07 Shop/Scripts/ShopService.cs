using UnityEngine;

public static class ShopService
{
    public static int GetMaxBuyCount(ItemData itemData, PlayerStat playerStat)
    {
        if (itemData == null || playerStat == null)
            return 1;

        if (itemData.buyPrice <= 0)
            return 999;

        int maxByGold = playerStat.Gold / itemData.buyPrice;
        return Mathf.Max(1, maxByGold);
    }

    public static bool TryBuy(
        int shopSlotIndex,
        int quantity,
        ShopUI shopUI,
        PlayerStat playerStat)
    {
        if (shopUI == null)
        {
            Debug.LogWarning("ShopUI가 연결되지 않았습니다.");
            return false;
        }

        if (playerStat == null)
        {
            Debug.LogWarning("PlayerStat이 연결되지 않았습니다.");
            return false;
        }

        if (InventoryManager.Instance == null)
        {
            Debug.LogWarning("InventoryManager가 없습니다.");
            return false;
        }

        if (quantity <= 0)
        {
            Debug.LogWarning("구매 수량이 올바르지 않습니다.");
            return false;
        }

        ItemData itemData = shopUI.GetItemDataByShopIndex(shopSlotIndex);
        if (itemData == null)
        {
            Debug.LogWarning($"상점 슬롯[{shopSlotIndex}]의 아이템을 찾을 수 없습니다.");
            return false;
        }

        int totalPrice = itemData.buyPrice * quantity;

        if (playerStat.Gold < totalPrice)
        {
            Debug.Log("골드가 부족합니다.");
            return false;
        }

        bool added = InventoryManager.Instance.AddItem(itemData, quantity);
        if (!added)
        {
            Debug.Log("인벤토리가 가득 차서 구매할 수 없습니다.");
            return false;
        }

        bool usedGold = playerStat.UseGold(totalPrice);
        if (!usedGold)
        {
            Debug.LogWarning("골드 차감 실패");
            InventoryManager.Instance.RemoveItem(itemData.itemId, quantity);
            return false;
        }

        ShopManager.Instance?.NotifyTradeSuccess();

        if (itemData != null)
            QuestService.NotifyBuyItem(itemData.itemName, quantity);

        Debug.Log($"구매 완료: {itemData.itemName} / 수량: {quantity} / 가격: {totalPrice}");
        SoundManager.Instance.PlaySFX(SfxType.BuySell);
        return true;
    }

    public static bool TrySell(
        int inventorySlotIndex,
        int quantity,
        PlayerStat playerStat)
    {
        if (playerStat == null)
        {
            Debug.LogWarning("PlayerStat이 연결되지 않았습니다.");
            return false;
        }

        if (InventoryManager.Instance == null)
        {
            Debug.LogWarning("InventoryManager가 없습니다.");
            return false;
        }

        if (quantity <= 0)
        {
            Debug.LogWarning("판매 수량이 올바르지 않습니다.");
            return false;
        }

        ItemData itemData = InventoryManager.Instance.GetItemAtSlot(inventorySlotIndex);
        if (itemData == null)
        {
            Debug.LogWarning($"인벤토리 슬롯[{inventorySlotIndex}]의 아이템을 찾을 수 없습니다.");
            return false;
        }

        int ownedCount = InventoryManager.Instance.GetItemCountAtSlot(inventorySlotIndex);
        if (ownedCount < quantity)
        {
            Debug.LogWarning($"판매 수량이 보유 수량보다 많습니다. 보유: {ownedCount}, 판매 요청: {quantity}");
            return false;
        }

        bool removed = InventoryManager.Instance.RemoveItemAtSlot(inventorySlotIndex, quantity);
        if (!removed)
        {
            Debug.LogWarning("아이템 제거 실패");
            return false;
        }

        int totalSellPrice = itemData.sellPrice * quantity;
        playerStat.AddGold(totalSellPrice);

        ShopManager.Instance?.NotifyTradeSuccess();

        if (itemData != null)
            QuestService.NotifySellItem(itemData.itemName, quantity);

        Debug.Log($"판매 완료: {itemData.itemName} / 수량: {quantity} / 가격: {totalSellPrice}");
        SoundManager.Instance.PlaySFX(SfxType.BuySell);
        return true;
    }
}