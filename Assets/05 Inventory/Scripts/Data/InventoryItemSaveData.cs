using System;

// 인벤토리 슬롯 저장 데이터
[Serializable]
public class InventoryItemSaveData
{
    public int slotIndex;   // 슬롯 위치
    public string itemId;   // 아이템 ID
    public int amount;      // 수량
}