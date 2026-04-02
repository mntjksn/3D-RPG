using System.Collections.Generic;

[System.Serializable]
public class PlayerSaveData
{
    public int level;
    public int currentExp;
    public float currentHp;
    public int gold;

    public List<InventoryItemSaveData> inventoryItems = new List<InventoryItemSaveData>();
}