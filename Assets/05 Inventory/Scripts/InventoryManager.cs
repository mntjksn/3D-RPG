using System;
using System.Collections.Generic;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance { get; private set; }

    [Header("Item Database")]
    [SerializeField] private List<ItemData> itemDatabase = new List<ItemData>();

    private readonly Dictionary<string, int> items = new Dictionary<string, int>();
    private readonly Dictionary<string, ItemData> itemLookup = new Dictionary<string, ItemData>();

    public IReadOnlyDictionary<string, int> Items => items;

    public event Action OnInventoryChanged;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        BuildItemLookup();
    }

    private void BuildItemLookup()
    {
        itemLookup.Clear();

        foreach (ItemData item in itemDatabase)
        {
            if (item == null || string.IsNullOrEmpty(item.itemId))
                continue;

            if (itemLookup.ContainsKey(item.itemId))
            {
                Debug.LogWarning($"중복된 itemId가 있습니다: {item.itemId}");
                continue;
            }

            itemLookup.Add(item.itemId, item);
        }
    }

    public void AddItem(ItemData itemData, int amount = 1)
    {
        if (itemData == null || string.IsNullOrEmpty(itemData.itemId) || amount <= 0)
            return;

        if (items.ContainsKey(itemData.itemId))
            items[itemData.itemId] += amount;
        else
            items.Add(itemData.itemId, amount);

        Debug.Log($"{itemData.itemName} {amount}개 획득. 현재 수량: {items[itemData.itemId]}");

        OnInventoryChanged?.Invoke();
    }

    public int GetItemCount(string itemId)
    {
        if (string.IsNullOrEmpty(itemId))
            return 0;

        return items.TryGetValue(itemId, out int count) ? count : 0;
    }

    public ItemData GetItemData(string itemId)
    {
        if (string.IsNullOrEmpty(itemId))
            return null;

        itemLookup.TryGetValue(itemId, out ItemData itemData);
        return itemData;
    }

    public List<InventoryItemSaveData> GetAllItems()
    {
        List<InventoryItemSaveData> list = new List<InventoryItemSaveData>();

        foreach (var pair in items)
        {
            list.Add(new InventoryItemSaveData
            {
                itemId = pair.Key,
                amount = pair.Value
            });
        }

        return list;
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
        {
            OnInventoryChanged?.Invoke();
            return;
        }

        foreach (var data in saveList)
        {
            if (data == null || string.IsNullOrEmpty(data.itemId))
                continue;

            items[data.itemId] = Mathf.Max(0, data.amount);
        }

        Debug.Log($"인벤토리 로드 완료: {items.Count}개");
        OnInventoryChanged?.Invoke();
    }
}