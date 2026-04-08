using System;
using System.Collections.Generic;
using UnityEngine;

public class EquipmentManager : MonoBehaviour
{
    public static EquipmentManager Instance { get; private set; }

    private readonly Dictionary<EquipmentSlotType, ItemData> equippedItems = new();

    public event Action OnEquipmentChanged;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void InitializeEquipment()
    {
        equippedItems.Clear();
        OnEquipmentChanged?.Invoke();
    }

    public ItemData GetEquippedItem(EquipmentSlotType slotType)
    {
        equippedItems.TryGetValue(slotType, out ItemData itemData);
        return itemData;
    }

    public bool EquipItemFromSlot(int slotIndex)
    {
        if (slotIndex < 0 || InventoryManager.Instance == null)
            return false;

        ItemData newItem = InventoryManager.Instance.GetItemAtSlot(slotIndex);

        if (newItem == null || !IsEquipable(newItem))
            return false;

        EquipmentSlotType slotType = newItem.equipSlot;
        ItemData oldItem = GetEquippedItem(slotType);

        bool removed = InventoryManager.Instance.RemoveItemAtSlot(slotIndex, 1);
        if (!removed)
            return false;

        if (oldItem != null)
            InventoryManager.Instance.AddItem(oldItem, 1);

        equippedItems[slotType] = newItem;

        Debug.Log($"장착 완료: {newItem.itemName} -> {slotType}");

        OnEquipmentChanged?.Invoke();
        MarkSaveDirty();
        return true;
    }

    public bool EquipItemDirect(ItemData newItem)
    {
        if (newItem == null || InventoryManager.Instance == null)
            return false;

        if (!IsEquipable(newItem))
            return false;

        EquipmentSlotType slotType = newItem.equipSlot;
        ItemData oldItem = GetEquippedItem(slotType);

        // 같은 아이템을 다시 같은 슬롯에 드롭한 경우
        if (oldItem != null && oldItem.itemId == newItem.itemId)
        {
            bool addedBack = InventoryManager.Instance.AddItem(oldItem, 1);
            if (!addedBack)
                return false;

            equippedItems.Remove(slotType);

            Debug.Log($"같은 장비 재드래그 -> 장착 해제: {oldItem.itemName}");

            OnEquipmentChanged?.Invoke();
            MarkSaveDirty();
            return true;
        }

        if (oldItem != null)
            InventoryManager.Instance.AddItem(oldItem, 1);

        equippedItems[slotType] = newItem;

        Debug.Log($"장착 교체 완료: {newItem.itemName} -> {slotType}");

        OnEquipmentChanged?.Invoke();
        MarkSaveDirty();
        return true;
    }

    public bool UnequipItem(EquipmentSlotType slotType)
    {
        if (!equippedItems.TryGetValue(slotType, out ItemData itemData) || itemData == null)
            return false;

        if (InventoryManager.Instance == null)
            return false;

        bool added = InventoryManager.Instance.AddItem(itemData, 1);
        if (!added)
            return false;

        equippedItems.Remove(slotType);

        Debug.Log($"장비 해제: {itemData.itemName}");

        OnEquipmentChanged?.Invoke();
        MarkSaveDirty();
        return true;
    }

    public bool UnequipSpecificItem(ItemData itemData)
    {
        if (itemData == null)
            return false;

        return UnequipItem(itemData.equipSlot);
    }

    public bool CanEquipToSlot(ItemData itemData, EquipmentSlotType slotType)
    {
        if (itemData == null)
            return false;

        if (!IsEquipable(itemData))
            return false;

        return itemData.equipSlot == slotType;
    }

    public EquipmentSaveData GetSaveData()
    {
        return new EquipmentSaveData
        {
            weaponItemId = GetEquippedItemId(EquipmentSlotType.Weapon),
            armorItemId = GetEquippedItemId(EquipmentSlotType.Armor),
            shoesItemId = GetEquippedItemId(EquipmentSlotType.Shoes),
            shieldItemId = GetEquippedItemId(EquipmentSlotType.Shield)
        };
    }

    public void LoadFromSaveData(EquipmentSaveData saveData)
    {
        equippedItems.Clear();

        if (saveData == null)
        {
            OnEquipmentChanged?.Invoke();
            return;
        }

        LoadEquippedItem(EquipmentSlotType.Weapon, saveData.weaponItemId);
        LoadEquippedItem(EquipmentSlotType.Armor, saveData.armorItemId);
        LoadEquippedItem(EquipmentSlotType.Shoes, saveData.shoesItemId);
        LoadEquippedItem(EquipmentSlotType.Shield, saveData.shieldItemId);

        Debug.Log("장비 로드 완료");
        OnEquipmentChanged?.Invoke();
    }

    private void LoadEquippedItem(EquipmentSlotType slotType, string itemId)
    {
        if (string.IsNullOrEmpty(itemId))
            return;

        if (InventoryManager.Instance == null)
            return;

        ItemData itemData = InventoryManager.Instance.GetItemData(itemId);
        if (itemData == null)
        {
            Debug.LogWarning($"장비 로드 실패: itemId에 해당하는 ItemData를 찾을 수 없음 - {itemId}");
            return;
        }

        equippedItems[slotType] = itemData;
    }

    private string GetEquippedItemId(EquipmentSlotType slotType)
    {
        ItemData itemData = GetEquippedItem(slotType);
        return itemData != null ? itemData.itemId : string.Empty;
    }

    private bool IsEquipable(ItemData itemData)
    {
        return itemData.itemType == ItemType.Weapon
            || itemData.itemType == ItemType.Armor
            || itemData.itemType == ItemType.Shoes
            || itemData.itemType == ItemType.Shield;
    }

    private void MarkSaveDirty()
    {
        if (SaveManager.Instance != null)
            SaveManager.Instance.MarkDirty();
    }
}