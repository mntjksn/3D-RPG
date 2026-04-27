using System;
using System.Collections.Generic;
using UnityEngine;

// 인벤토리 데이터 관리 및 저장 담당
public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance { get; private set; }

    [Header("Item Database")]
    [SerializeField] private List<ItemData> itemDatabase = new();

    [Header("Inventory Setting")]
    [SerializeField] private int maxSlotCount = 20;

    private readonly List<InventorySlotData> slots = new();
    private readonly Dictionary<string, ItemData> itemLookup = new();

    public IReadOnlyList<InventorySlotData> Slots => slots;
    public int MaxSlotCount => maxSlotCount;

    public event Action OnInventoryChanged;

    private void Awake()
    {
        // 싱글톤 설정
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

    // 아이템 lookup 테이블 생성
    private void BuildItemLookup()
    {
        itemLookup.Clear();

        foreach (ItemData item in itemDatabase)
        {
            if (item == null || string.IsNullOrEmpty(item.itemId)) continue;
            if (itemLookup.ContainsKey(item.itemId)) continue;

            itemLookup.Add(item.itemId, item);
        }
    }

    // 인벤 초기화
    public void InitializeInventory()
    {
        slots.Clear();

        for (int i = 0; i < maxSlotCount; i++)
            slots.Add(new InventorySlotData());

        OnInventoryChanged?.Invoke();
    }

    // ItemData 조회
    public ItemData GetItemData(string itemId)
    {
        if (string.IsNullOrEmpty(itemId)) return null;

        itemLookup.TryGetValue(itemId, out ItemData itemData);
        return itemData;
    }

    public InventorySlotData GetSlot(int slotIndex)
    {
        if (!IsValidSlotIndex(slotIndex)) return null;
        return slots[slotIndex];
    }

    public ItemData GetItemAtSlot(int slotIndex)
    {
        if (!IsValidSlotIndex(slotIndex)) return null;

        InventorySlotData slot = slots[slotIndex];
        if (slot == null || slot.IsEmpty()) return null;

        return GetItemData(slot.itemId);
    }

    public int GetItemCountAtSlot(int slotIndex)
    {
        if (!IsValidSlotIndex(slotIndex)) return 0;

        InventorySlotData slot = slots[slotIndex];
        return slot != null ? slot.amount : 0;
    }

    // 전체 아이템 개수 계산
    public int GetItemCount(string itemId)
    {
        if (string.IsNullOrEmpty(itemId)) return 0;

        int total = 0;

        for (int i = 0; i < slots.Count; i++)
        {
            InventorySlotData slot = slots[i];
            if (slot == null || slot.IsEmpty()) continue;

            if (slot.itemId == itemId)
                total += slot.amount;
        }

        return total;
    }

    // 아이템 추가
    public bool AddItem(ItemData itemData, int amount = 1)
    {
        if (itemData == null || string.IsNullOrEmpty(itemData.itemId) || amount <= 0)
            return false;

        int remain = amount;
        int maxStack = GetMaxStack(itemData);

        // 스택 가능한 슬롯에 먼저 추가
        if (maxStack > 1)
        {
            for (int i = 0; i < slots.Count; i++)
            {
                InventorySlotData slot = slots[i];
                if (slot.IsEmpty()) continue;
                if (slot.itemId != itemData.itemId) continue;
                if (slot.amount >= maxStack) continue;

                int canAdd = maxStack - slot.amount;
                int addAmount = Mathf.Min(canAdd, remain);

                slot.amount += addAmount;
                remain -= addAmount;

                if (remain <= 0) break;
            }
        }

        // 빈 슬롯에 추가
        while (remain > 0)
        {
            int emptyIndex = FindFirstEmptySlot();
            if (emptyIndex < 0)
            {
                OnInventoryChanged?.Invoke();
                return false;
            }

            int addAmount = maxStack > 1 ? Mathf.Min(maxStack, remain) : 1;

            slots[emptyIndex].itemId = itemData.itemId;
            slots[emptyIndex].amount = addAmount;

            remain -= addAmount;
        }

        OnInventoryChanged?.Invoke();
        MarkSaveDirty();
        return true;
    }

    // 아이템 제거
    public bool RemoveItem(string itemId, int amount = 1)
    {
        if (string.IsNullOrEmpty(itemId) || amount <= 0) return false;
        if (GetItemCount(itemId) < amount) return false;

        int remain = amount;

        for (int i = 0; i < slots.Count; i++)
        {
            InventorySlotData slot = slots[i];
            if (slot.IsEmpty()) continue;
            if (slot.itemId != itemId) continue;

            int removeAmount = Mathf.Min(slot.amount, remain);
            slot.amount -= removeAmount;
            remain -= removeAmount;

            if (slot.amount <= 0)
                slot.Clear();

            if (remain <= 0) break;
        }

        CompactInventory();

        OnInventoryChanged?.Invoke();
        MarkSaveDirty();
        return true;
    }

    public bool RemoveItemAtSlot(int slotIndex, int amount = 1)
    {
        if (!IsValidSlotIndex(slotIndex) || amount <= 0) return false;

        InventorySlotData slot = slots[slotIndex];
        if (slot == null || slot.IsEmpty()) return false;
        if (slot.amount < amount) return false;

        slot.amount -= amount;

        if (slot.amount <= 0)
            slot.Clear();

        CompactInventory();

        OnInventoryChanged?.Invoke();
        MarkSaveDirty();
        return true;
    }

    // 슬롯 교환
    public bool SwapSlots(int fromIndex, int toIndex)
    {
        if (!IsValidSlotIndex(fromIndex) || !IsValidSlotIndex(toIndex)) return false;
        if (fromIndex == toIndex) return false;

        InventorySlotData temp = new(slots[fromIndex].itemId, slots[fromIndex].amount);

        slots[fromIndex].itemId = slots[toIndex].itemId;
        slots[fromIndex].amount = slots[toIndex].amount;

        slots[toIndex].itemId = temp.itemId;
        slots[toIndex].amount = temp.amount;

        OnInventoryChanged?.Invoke();
        MarkSaveDirty();
        return true;
    }

    // 인벤 정리 (스택 정렬)
    public void CompactInventory()
    {
        Dictionary<string, int> stackedItems = new();

        for (int i = 0; i < slots.Count; i++)
        {
            InventorySlotData slot = slots[i];
            if (slot == null || slot.IsEmpty()) continue;

            if (!stackedItems.ContainsKey(slot.itemId))
                stackedItems[slot.itemId] = 0;

            stackedItems[slot.itemId] += slot.amount;
        }

        slots.Clear();

        foreach (var pair in stackedItems)
        {
            ItemData itemData = GetItemData(pair.Key);
            if (itemData == null) continue;

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

    // 저장 데이터 생성
    public List<InventoryItemSaveData> GetSaveData()
    {
        List<InventoryItemSaveData> saveList = new();

        for (int i = 0; i < slots.Count; i++)
        {
            InventorySlotData slot = slots[i];
            if (slot == null || slot.IsEmpty()) continue;

            saveList.Add(new InventoryItemSaveData
            {
                slotIndex = i,
                itemId = slot.itemId,
                amount = slot.amount
            });
        }

        return saveList;
    }

    // 저장 데이터 로드
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
            if (data == null) continue;
            if (string.IsNullOrEmpty(data.itemId) || data.amount <= 0) continue;
            if (!IsValidSlotIndex(data.slotIndex)) continue;

            slots[data.slotIndex].itemId = data.itemId;
            slots[data.slotIndex].amount = data.amount;
        }

        CompactInventory();

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
        if (itemData.itemType == ItemType.Weapon ||
            itemData.itemType == ItemType.Armor ||
            itemData.itemType == ItemType.Shoes ||
            itemData.itemType == ItemType.Shield)
        {
            return 1;
        }

        return itemData.maxStack;
    }

    private void MarkSaveDirty()
    {
        SaveManager.Instance?.MarkDirty();
    }
}