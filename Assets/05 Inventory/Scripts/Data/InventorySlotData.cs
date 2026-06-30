using System;

// 인벤토리 슬롯 데이터 (런타임)
[Serializable]
public class InventorySlotData
{
    public string itemId;
    public int amount;

    public InventorySlotData()
    {
        itemId = string.Empty;
        amount = 0;
    }

    public InventorySlotData(string itemId, int amount)
    {
        this.itemId = itemId;
        this.amount = amount;
    }

    // 슬롯이 비어있는지 확인
    public bool IsEmpty()
    {
        return string.IsNullOrEmpty(itemId) || amount <= 0;
    }

    // 슬롯 초기화
    public void Clear()
    {
        itemId = string.Empty;
        amount = 0;
    }
}