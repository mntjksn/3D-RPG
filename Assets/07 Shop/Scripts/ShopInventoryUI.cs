using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShopInventoryUI : MonoBehaviour
{
    [Header("Slot Settings")]
    [SerializeField] private ShopInventorySlotUI slotPrefab;
    [SerializeField] private Transform slotParent;
    [SerializeField] private int slotCount = 20;

    [Header("Tab Buttons")]
    [SerializeField] private Button equipmentButton;
    [SerializeField] private Button itemButton;

    private readonly List<ShopInventorySlotUI> slots = new List<ShopInventorySlotUI>();
    private InventoryTabType currentTab = InventoryTabType.Equipment;
    private bool isInitialized;

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

        currentTab = InventoryTabType.Equipment;
        RefreshUI();
    }

    private void OnDisable()
    {
        if (InventoryManager.Instance != null)
            InventoryManager.Instance.OnInventoryChanged -= RefreshUI;
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
        ClearSlots();

        for (int i = 0; i < slotCount; i++)
        {
            ShopInventorySlotUI slot = Instantiate(slotPrefab, slotParent);
            slot.SetIndex(-1);
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
            Destroy(slotParent.GetChild(i).gameObject);
    }

    public void RefreshUI()
    {
        if (!isInitialized)
            return;

        for (int i = 0; i < slots.Count; i++)
        {
            slots[i].SetIndex(-1);
            slots[i].SetEmpty();
        }

        if (InventoryManager.Instance == null)
            return;

        var inventorySlots = InventoryManager.Instance.Slots;
        int displayIndex = 0;

        for (int realIndex = 0; realIndex < inventorySlots.Count; realIndex++)
        {
            InventorySlotData slotData = inventorySlots[realIndex];

            if (slotData == null || slotData.IsEmpty())
                continue;

            ItemData itemData = InventoryManager.Instance.GetItemData(slotData.itemId);
            if (itemData == null)
                continue;

            bool canShow = currentTab == InventoryTabType.Equipment
                ? IsEquipment(itemData)
                : IsItem(itemData);

            if (!canShow)
                continue;

            if (displayIndex >= slots.Count)
                break;

            slots[displayIndex].SetIndex(realIndex);
            slots[displayIndex].SetItem(itemData, slotData.amount);
            displayIndex++;
        }
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
        return itemData.itemType == ItemType.Material
            || itemData.itemType == ItemType.Consumable;
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