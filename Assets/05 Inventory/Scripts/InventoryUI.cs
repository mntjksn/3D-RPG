using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InventoryUI : MonoBehaviour
{
    [Header("Main Panel")]
    [SerializeField] private GameObject closePanel;

    [Header("Slot Settings")]
    [SerializeField] private InventorySlotUI slotPrefab;
    [SerializeField] private Transform slotParent;
    [SerializeField] private int slotCount = 20;

    [Header("Tab Buttons")]
    [SerializeField] private Button equipmentButton;
    [SerializeField] private Button itemButton;
    [SerializeField] private Button closeButton;

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

        if (closeButton != null)
            closeButton.onClick.AddListener(CloseInventory);
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
            slot.SetIndex(-1);   // 실제 인벤토리 슬롯 번호는 RefreshUI에서 넣음
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

            // 화면에는 앞에서부터 채우되,
            // 이 슬롯이 실제 인벤토리 몇 번 슬롯인지 따로 저장
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

    public void CloseInventory()
    {
        if (closePanel != null)
            UIManager.Instance.ClosePanel(UIPanelType.Inventory);
    }
}