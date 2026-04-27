using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// 인벤 UI 생성, 탭 전환, 슬롯 표시 담당
public class InventoryUI : MonoBehaviour
{
    [Header("Slot Settings")]
    [SerializeField] private InventorySlotUI slotPrefab;
    [SerializeField] private Transform slotParent;
    [SerializeField] private int slotCount = 20;

    [Header("Tab Buttons")]
    [SerializeField] private Button equipmentButton;
    [SerializeField] private Button itemButton;
    [SerializeField] private Button closeButton;

    private readonly List<InventorySlotUI> slots = new();
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

    // 버튼 이벤트 연결
    private void BindButtons()
    {
        equipmentButton?.onClick.AddListener(OnClickEquipmentTab);
        itemButton?.onClick.AddListener(OnClickItemTab);
        closeButton?.onClick.AddListener(CloseInventory);
    }

    // 슬롯 생성
    private void CreateSlots()
    {
        if (slotPrefab == null || slotParent == null)
            return;

        ClearSlots();

        for (int i = 0; i < slotCount; i++)
        {
            InventorySlotUI slot = Instantiate(slotPrefab, slotParent);
            slot.SetIndex(-1);
            slot.SetEmpty();
            slots.Add(slot);
        }
    }

    // 기존 슬롯 제거
    private void ClearSlots()
    {
        slots.Clear();

        if (slotParent == null)
            return;

        for (int i = slotParent.childCount - 1; i >= 0; i--)
            Destroy(slotParent.GetChild(i).gameObject);
    }

    // 현재 탭 기준으로 슬롯 갱신
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

            // 화면 슬롯과 실제 인벤 슬롯 번호 연결
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

    // 장비 탭 열기
    public void OnClickEquipmentTab()
    {
        currentTab = InventoryTabType.Equipment;
        RefreshUI();
    }

    // 아이템 탭 열기
    public void OnClickItemTab()
    {
        currentTab = InventoryTabType.Item;
        RefreshUI();
    }

    // 인벤 닫기
    public void CloseInventory()
    {
        UIManager.Instance?.ClosePanel(UIPanelType.Inventory);
    }
}