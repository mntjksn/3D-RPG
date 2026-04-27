using System;
using System.Collections.Generic;
using UnityEngine;

// 장비 장착, 해제, 저장 데이터 관리 담당
public class EquipmentManager : MonoBehaviour
{
    public static EquipmentManager Instance { get; private set; }

    private readonly Dictionary<EquipmentSlotType, ItemData> equippedItems = new();

    public event Action OnEquipmentChanged;

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
    }

    // 장비 초기화
    public void InitializeEquipment()
    {
        equippedItems.Clear();
        OnEquipmentChanged?.Invoke();
    }

    // 장착된 아이템 조회
    public ItemData GetEquippedItem(EquipmentSlotType slotType)
    {
        equippedItems.TryGetValue(slotType, out ItemData itemData);
        return itemData;
    }

    // 인벤토리 슬롯에서 장착
    public bool EquipItemFromSlot(int slotIndex)
    {
        if (slotIndex < 0 || InventoryManager.Instance == null) return false;

        ItemData newItem = InventoryManager.Instance.GetItemAtSlot(slotIndex);
        if (newItem == null || !IsEquipable(newItem)) return false;

        EquipmentSlotType slotType = newItem.equipSlot;
        ItemData oldItem = GetEquippedItem(slotType);

        if (!InventoryManager.Instance.RemoveItemAtSlot(slotIndex, 1))
            return false;

        if (oldItem != null)
            InventoryManager.Instance.AddItem(oldItem, 1);

        equippedItems[slotType] = newItem;

        SoundManager.Instance?.PlaySFX(SfxType.Equip);
        QuestService.NotifyEquipItem(newItem.itemName);

        OnEquipmentChanged?.Invoke();
        MarkSaveDirty();
        return true;
    }

    // 드래그로 바로 장착
    public bool EquipItemDirect(ItemData newItem)
    {
        if (newItem == null || InventoryManager.Instance == null) return false;
        if (!IsEquipable(newItem)) return false;

        EquipmentSlotType slotType = newItem.equipSlot;
        ItemData oldItem = GetEquippedItem(slotType);

        // 같은 아이템 다시 드롭 → 장착 해제
        if (oldItem != null && oldItem.itemId == newItem.itemId)
        {
            if (!InventoryManager.Instance.AddItem(oldItem, 1))
                return false;

            equippedItems.Remove(slotType);

            SoundManager.Instance?.PlaySFX(SfxType.Equip);

            OnEquipmentChanged?.Invoke();
            MarkSaveDirty();
            return true;
        }

        if (oldItem != null)
            InventoryManager.Instance.AddItem(oldItem, 1);

        equippedItems[slotType] = newItem;

        SoundManager.Instance?.PlaySFX(SfxType.Equip);

        OnEquipmentChanged?.Invoke();
        MarkSaveDirty();
        return true;
    }

    // 장비 해제
    public bool UnequipItem(EquipmentSlotType slotType)
    {
        if (!equippedItems.TryGetValue(slotType, out ItemData itemData) || itemData == null)
            return false;

        if (InventoryManager.Instance == null)
            return false;

        if (!InventoryManager.Instance.AddItem(itemData, 1))
            return false;

        equippedItems.Remove(slotType);

        SoundManager.Instance?.PlaySFX(SfxType.Equip);

        OnEquipmentChanged?.Invoke();
        MarkSaveDirty();
        return true;
    }

    public bool UnequipSpecificItem(ItemData itemData)
    {
        if (itemData == null) return false;
        return UnequipItem(itemData.equipSlot);
    }

    // 슬롯에 장착 가능한지 확인
    public bool CanEquipToSlot(ItemData itemData, EquipmentSlotType slotType)
    {
        if (itemData == null) return false;
        if (!IsEquipable(itemData)) return false;

        return itemData.equipSlot == slotType;
    }

    // 저장 데이터 생성
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

    // 저장 데이터 로드
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

        OnEquipmentChanged?.Invoke();
    }

    private void LoadEquippedItem(EquipmentSlotType slotType, string itemId)
    {
        if (string.IsNullOrEmpty(itemId)) return;
        if (InventoryManager.Instance == null) return;

        ItemData itemData = InventoryManager.Instance.GetItemData(itemId);
        if (itemData == null) return;

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
        SaveManager.Instance?.MarkDirty();
    }
}