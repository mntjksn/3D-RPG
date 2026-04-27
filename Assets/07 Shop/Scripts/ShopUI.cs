using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// 상점 슬롯 생성, 탭 전환, 아이템 표시 담당
public class ShopUI : MonoBehaviour
{
    [Header("Shop Data")]
    [SerializeField] private ShopData shopData;

    [Header("Slot Settings")]
    [SerializeField] private ShopSlotUI slotPrefab;
    [SerializeField] private Transform slotParent;
    [SerializeField] private int slotCount = 20;

    [Header("Tab Buttons")]
    [SerializeField] private Button equipmentButton;
    [SerializeField] private Button itemButton;

    private readonly List<ShopSlotUI> slots = new();
    private ShopTabType currentTab = ShopTabType.Equipment;
    private bool isInitialized;

    public IReadOnlyList<ShopSlotUI> Slots => slots;

    private void Start()
    {
        CreateSlots();
        BindButtons();
        isInitialized = true;
        RefreshUI();
    }

    private void OnEnable()
    {
        currentTab = ShopTabType.Equipment;
        RefreshUI();
    }

    // 버튼 이벤트 연결
    private void BindButtons()
    {
        equipmentButton?.onClick.AddListener(OnClickEquipmentTab);
        itemButton?.onClick.AddListener(OnClickItemTab);
    }

    // 슬롯 생성
    private void CreateSlots()
    {
        if (slotPrefab == null || slotParent == null)
            return;

        ClearSlots();

        for (int i = 0; i < slotCount; i++)
        {
            ShopSlotUI slot = Instantiate(slotPrefab, slotParent);
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

        if (shopData == null || shopData.items == null)
            return;

        int displayIndex = 0;

        for (int realIndex = 0; realIndex < shopData.items.Count; realIndex++)
        {
            ShopItemEntry entry = shopData.items[realIndex];
            if (entry == null || entry.itemData == null)
                continue;

            bool canShow = currentTab == ShopTabType.Equipment
                ? IsEquipment(entry.itemData)
                : IsItem(entry.itemData);

            if (!canShow)
                continue;

            if (displayIndex >= slots.Count)
                break;

            slots[displayIndex].SetIndex(realIndex);
            slots[displayIndex].SetItem(entry.itemData);
            displayIndex++;
        }
    }

    // 상점 인덱스로 아이템 조회
    public ItemData GetItemDataByShopIndex(int shopIndex)
    {
        if (shopData == null || shopData.items == null)
            return null;

        if (shopIndex < 0 || shopIndex >= shopData.items.Count)
            return null;

        return shopData.items[shopIndex]?.itemData;
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
        currentTab = ShopTabType.Equipment;
        RefreshUI();
    }

    // 아이템 탭 열기
    public void OnClickItemTab()
    {
        currentTab = ShopTabType.Item;
        RefreshUI();
    }
}