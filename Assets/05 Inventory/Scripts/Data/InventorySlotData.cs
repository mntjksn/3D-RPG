using System;

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

    public bool IsEmpty()
    {
        return string.IsNullOrEmpty(itemId) || amount <= 0;
    }

    public void Clear()
    {
        itemId = string.Empty;
        amount = 0;
    }
}