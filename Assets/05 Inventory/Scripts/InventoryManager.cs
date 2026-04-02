using System.Collections.Generic;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance { get; private set; }

    private Dictionary<string, int> items = new Dictionary<string, int>();

    public IReadOnlyDictionary<string, int> Items => items;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public void AddItem(ItemData itemData, int amount = 1)
    {
        if (itemData == null || string.IsNullOrEmpty(itemData.itemId) || amount <= 0)
            return;

        if (items.ContainsKey(itemData.itemId))
            items[itemData.itemId] += amount;
        else
            items.Add(itemData.itemId, amount);

        Debug.Log($"{itemData.itemName} {amount}°³ È¹µæ. ÇöÀç ¼ö·®: {items[itemData.itemId]}");
    }

    public int GetItemCount(string itemId)
    {
        if (string.IsNullOrEmpty(itemId))
            return 0;

        return items.TryGetValue(itemId, out int count) ? count : 0;
    }

    public List<InventoryItemSaveData> GetSaveData()
    {
        List<InventoryItemSaveData> saveList = new List<InventoryItemSaveData>();

        foreach (var pair in items)
        {
            saveList.Add(new InventoryItemSaveData
            {
                itemId = pair.Key,
                amount = pair.Value
            });
        }

        return saveList;
    }

    public void LoadFromSaveData(List<InventoryItemSaveData> saveList)
    {
        items.Clear();

        if (saveList == null)
            return;

        foreach (var data in saveList)
        {
            if (data == null || string.IsNullOrEmpty(data.itemId))
                continue;

            items[data.itemId] = Mathf.Max(0, data.amount);
        }
    }
}