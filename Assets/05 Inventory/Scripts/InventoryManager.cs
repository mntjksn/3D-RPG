using System;
using System.Collections.Generic;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance { get; private set; }

    [Header("Item Database")]
    [SerializeField] private List<ItemData> itemDatabase = new List<ItemData>();

    [Header("Inventory Setting")]
    [SerializeField] private int maxSlotCount = 20;

    private readonly List<InventorySlotData> slots = new List<InventorySlotData>();
    private readonly Dictionary<string, ItemData> itemLookup = new Dictionary<string, ItemData>();

    public IReadOnlyList<InventorySlotData> Slots => slots;
    public int MaxSlotCount => maxSlotCount;

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
        InitializeInventory();
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

    public void InitializeInventory()
    {
        slots.Clear();

        for (int i = 0; i < maxSlotCount; i++)
            slots.Add(new InventorySlotData());

        OnInventoryChanged?.Invoke();
    }

    public ItemData GetItemData(string itemId)
    {
        if (string.IsNullOrEmpty(itemId))
            return null;

        itemLookup.TryGetValue(itemId, out ItemData itemData);
        return itemData;
    }

    public InventorySlotData GetSlot(int slotIndex)
    {
        if (!IsValidSlotIndex(slotIndex))
            return null;

        return slots[slotIndex];
    }

    public ItemData GetItemAtSlot(int slotIndex)
    {
        if (!IsValidSlotIndex(slotIndex))
            return null;

        InventorySlotData slot = slots[slotIndex];
        if (slot == null || slot.IsEmpty())
            return null;

        return GetItemData(slot.itemId);
    }

    public int GetItemCountAtSlot(int slotIndex)
    {
        if (!IsValidSlotIndex(slotIndex))
            return 0;

        InventorySlotData slot = slots[slotIndex];
        return slot != null ? slot.amount : 0;
    }

    public int GetItemCount(string itemId)
    {
        if (string.IsNullOrEmpty(itemId))
            return 0;

        int total = 0;

        for (int i = 0; i < slots.Count; i++)
        {
            InventorySlotData slot = slots[i];
            if (slot == null || slot.IsEmpty())
                continue;

            if (slot.itemId == itemId)
                total += slot.amount;
        }

        return total;
    }

    public bool AddItem(ItemData itemData, int amount = 1)
    {
        if (itemData == null || string.IsNullOrEmpty(itemData.itemId) || amount <= 0)
            return false;

        int remain = amount;

        // 1. 같은 아이템 스택 가능한 슬롯 먼저 찾기
        // ItemData에 maxStack 같은 값이 없다면 사실상 1칸 1개 처리
        int maxStack = GetMaxStack(itemData);

        if (maxStack > 1)
        {
            for (int i = 0; i < slots.Count; i++)
            {
                InventorySlotData slot = slots[i];
                if (slot.IsEmpty())
                    continue;

                if (slot.itemId != itemData.itemId)
                    continue;

                if (slot.amount >= maxStack)
                    continue;

                int canAdd = maxStack - slot.amount;
                int addAmount = Mathf.Min(canAdd, remain);

                slot.amount += addAmount;
                remain -= addAmount;

                if (remain <= 0)
                    break;
            }
        }

        // 2. 빈 슬롯에 추가
        while (remain > 0)
        {
            int emptyIndex = FindFirstEmptySlot();
            if (emptyIndex < 0)
            {
                Debug.LogWarning("인벤토리가 가득 찼습니다.");
                OnInventoryChanged?.Invoke();
                return false;
            }

            int addAmount = maxStack > 1 ? Mathf.Min(maxStack, remain) : 1;

            slots[emptyIndex].itemId = itemData.itemId;
            slots[emptyIndex].amount = addAmount;

            remain -= addAmount;
        }

        Debug.Log($"{itemData.itemName} {amount}개 획득");
        OnInventoryChanged?.Invoke();
        MarkSaveDirty();
        return true;
    }

    public bool RemoveItem(string itemId, int amount = 1)
    {
        if (string.IsNullOrEmpty(itemId) || amount <= 0)
            return false;

        if (GetItemCount(itemId) < amount)
            return false;

        int remain = amount;

        for (int i = 0; i < slots.Count; i++)
        {
            InventorySlotData slot = slots[i];
            if (slot.IsEmpty())
                continue;

            if (slot.itemId != itemId)
                continue;

            int removeAmount = Mathf.Min(slot.amount, remain);
            slot.amount -= removeAmount;
            remain -= removeAmount;

            if (slot.amount <= 0)
                slot.Clear();

            if (remain <= 0)
                break;
        }

        CompactInventory();

        OnInventoryChanged?.Invoke();
        MarkSaveDirty();
        return true;
    }

    public bool RemoveItemAtSlot(int slotIndex, int amount = 1)
    {
        if (!IsValidSlotIndex(slotIndex) || amount <= 0)
            return false;

        InventorySlotData slot = slots[slotIndex];
        if (slot == null || slot.IsEmpty())
            return false;

        if (slot.amount < amount)
            return false;

        slot.amount -= amount;

        if (slot.amount <= 0)
            slot.Clear();

        CompactInventory();

        OnInventoryChanged?.Invoke();
        MarkSaveDirty();
        return true;
    }

    public bool SwapSlots(int fromIndex, int toIndex)
    {
        if (!IsValidSlotIndex(fromIndex) || !IsValidSlotIndex(toIndex))
            return false;

        if (fromIndex == toIndex)
            return false;

        InventorySlotData temp = new InventorySlotData(slots[fromIndex].itemId, slots[fromIndex].amount);

        slots[fromIndex].itemId = slots[toIndex].itemId;
        slots[fromIndex].amount = slots[toIndex].amount;

        slots[toIndex].itemId = temp.itemId;
        slots[toIndex].amount = temp.amount;

        OnInventoryChanged?.Invoke();
        MarkSaveDirty();
        return true;
    }

    public void CompactInventory()
    {
        Dictionary<string, int> stackedItems = new Dictionary<string, int>();

        for (int i = 0; i < slots.Count; i++)
        {
            InventorySlotData slot = slots[i];
            if (slot == null || slot.IsEmpty())
                continue;

            if (!stackedItems.ContainsKey(slot.itemId))
                stackedItems[slot.itemId] = 0;

            stackedItems[slot.itemId] += slot.amount;
        }

        slots.Clear();

        foreach (var pair in stackedItems)
        {
            ItemData itemData = GetItemData(pair.Key);
            if (itemData == null)
                continue;

            int remain = pair.Value;
            int maxStack = GetMaxStack(itemData);

            while (remain > 0)
            {
                int addAmount = Mathf.Min(maxStack, remain);
                slots.Add(new InventorySlotData(pair.Key, addAmount));
                remain -= addAmount;
            }
        }

        while (slots.Count < maxSlotCount)
            slots.Add(new InventorySlotData());
    }

    public List<InventoryItemSaveData> GetSaveData()
    {
        List<InventoryItemSaveData> saveList = new List<InventoryItemSaveData>();

        for (int i = 0; i < slots.Count; i++)
        {
            InventorySlotData slot = slots[i];

            if (slot == null || slot.IsEmpty())
                continue;

            saveList.Add(new InventoryItemSaveData
            {
                slotIndex = i,
                itemId = slot.itemId,
                amount = slot.amount
            });
        }

        return saveList;
    }

    public void LoadFromSaveData(List<InventoryItemSaveData> saveList)
    {
        InitializeInventory();

        if (saveList == null)
        {
            OnInventoryChanged?.Invoke();
            return;
        }

        foreach (InventoryItemSaveData data in saveList)
        {
            if (data == null)
                continue;

            if (string.IsNullOrEmpty(data.itemId) || data.amount <= 0)
                continue;

            if (!IsValidSlotIndex(data.slotIndex))
                continue;

            slots[data.slotIndex].itemId = data.itemId;
            slots[data.slotIndex].amount = data.amount;
        }

        CompactInventory();

        Debug.Log($"인벤토리 로드 완료: {saveList.Count}개");
        OnInventoryChanged?.Invoke();
    }

    private int FindFirstEmptySlot()
    {
        for (int i = 0; i < slots.Count; i++)
        {
            if (slots[i] == null || slots[i].IsEmpty())
                return i;
        }

        return -1;
    }

    private bool IsValidSlotIndex(int slotIndex)
    {
        return slotIndex >= 0 && slotIndex < slots.Count;
    }

    private int GetMaxStack(ItemData itemData)
    {
        // 네 ItemData에 maxStack이 있으면 그걸 쓰면 됨
        // 예: return itemData.maxStack;

        // 장비류는 보통 1개씩
        if (itemData.itemType == ItemType.Weapon ||
            itemData.itemType == ItemType.Armor ||
            itemData.itemType == ItemType.Shoes ||
            itemData.itemType == ItemType.Shield)
        {
            return 1;
        }

        // 나머지는 임시로 99
        return itemData.maxStack;
    }

    private void MarkSaveDirty()
    {
        if (SaveManager.Instance != null)
            SaveManager.Instance.MarkDirty();
    }
}