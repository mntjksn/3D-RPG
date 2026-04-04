using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InventoryUI : MonoBehaviour
{
    [Header("Slot Settings")]
    [SerializeField] private InventorySlotUI slotPrefab;
    [SerializeField] private Transform slotParent;
    [SerializeField] private int slotCount = 20;

    [Header("Tab Buttons")]
    [SerializeField] private Button equipmentButton;
    [SerializeField] private Button itemButton;

    private readonly List<InventorySlotUI> slots = new List<InventorySlotUI>();
    private InventoryTabType currentTab = InventoryTabType.Equipment;
    private bool isInitialized;

    public IReadOnlyList<InventorySlotUI> Slots => slots;

    private void Start()
    {
        CreateSlots();
        BindButtons();
        isInitialized = true;
        RefreshUI();
    }

    private void OnEnable()
    {
        if (InventoryManager.Instance != null)
            InventoryManager.Instance.OnInventoryChanged += RefreshUI;

        Cursor.lockState = CursorLockMode.Confined;
    }

    private void OnDisable()
    {
        if (InventoryManager.Instance != null)
            InventoryManager.Instance.OnInventoryChanged -= RefreshUI;

        Cursor.lockState = CursorLockMode.Locked;
    }

    private void BindButtons()
    {
        if (equipmentButton != null)
            equipmentButton.onClick.AddListener(OnClickEquipmentTab);

        if (itemButton != null)
            itemButton.onClick.AddListener(OnClickItemTab);
    }

    private void CreateSlots()
    {
        if (slotPrefab == null)
        {
            Debug.LogWarning("slotPrefab이 비어 있습니다.");
            return;
        }

        if (slotParent == null)
        {
            Debug.LogWarning("slotParent가 비어 있습니다.");
            return;
        }

        ClearSlots();

        for (int i = 0; i < slotCount; i++)
        {
            InventorySlotUI slot = Instantiate(slotPrefab, slotParent);
            slot.SetIndex(i);
            slot.SetEmpty();
            slots.Add(slot);
        }
    }

    private void ClearSlots()
    {
        slots.Clear();

        if (slotParent == null)
            return;

        for (int i = slotParent.childCount - 1; i >= 0; i--)
        {
            Destroy(slotParent.GetChild(i).gameObject);
        }
    }

    public void RefreshUI()
    {
        if (!isInitialized)
            return;

        foreach (InventorySlotUI slot in slots)
            slot.SetEmpty();

        if (InventoryManager.Instance == null)
            return;

        List<InventoryItemSaveData> filteredItems = GetFilteredItems();

        for (int i = 0; i < filteredItems.Count && i < slots.Count; i++)
        {
            InventoryItemSaveData saveData = filteredItems[i];
            ItemData itemData = InventoryManager.Instance.GetItemData(saveData.itemId);

            if (itemData == null)
                continue;

            slots[i].SetItem(itemData, saveData.amount);
        }
    }

    private List<InventoryItemSaveData> GetFilteredItems()
    {
        List<InventoryItemSaveData> result = new List<InventoryItemSaveData>();

        if (InventoryManager.Instance == null)
            return result;

        List<InventoryItemSaveData> allItems = InventoryManager.Instance.GetAllItems();

        foreach (InventoryItemSaveData saveData in allItems)
        {
            ItemData itemData = InventoryManager.Instance.GetItemData(saveData.itemId);

            if (itemData == null)
                continue;

            if (currentTab == InventoryTabType.Equipment)
            {
                if (IsEquipment(itemData))
                    result.Add(saveData);
            }
            else
            {
                if (IsItem(itemData))
                    result.Add(saveData);
            }
        }

        return result;
    }

    private bool IsEquipment(ItemData itemData)
    {
        return itemData.itemType == ItemType.Weapon
            || itemData.itemType == ItemType.Armor
            || itemData.itemType == ItemType.Shoes
            || itemData.itemType == ItemType.Shield;
    }

    private bool IsItem(ItemData itemData)
    {
        return itemData.itemType == ItemType.Material;
    }

    public void OnClickEquipmentTab()
    {
        currentTab = InventoryTabType.Equipment;
        RefreshUI();
    }

    public void OnClickItemTab()
    {
        currentTab = InventoryTabType.Item;
        RefreshUI();
    }
}