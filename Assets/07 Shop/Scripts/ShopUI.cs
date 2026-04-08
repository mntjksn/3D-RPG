using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

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

    private readonly List<ShopSlotUI> slots = new List<ShopSlotUI>();
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
            Debug.LogWarning("ShopUI slotPrefab이 비어 있습니다.");
            return;
        }

        if (slotParent == null)
        {
            Debug.LogWarning("ShopUI slotParent가 비어 있습니다.");
            return;
        }

        ClearSlots();

        for (int i = 0; i < slotCount; i++)
        {
            ShopSlotUI slot = Instantiate(slotPrefab, slotParent);
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
        return itemData.itemType == ItemType.Material;
    }

    public void OnClickEquipmentTab()
    {
        currentTab = ShopTabType.Equipment;
        RefreshUI();
    }

    public void OnClickItemTab()
    {
        currentTab = ShopTabType.Item;
        RefreshUI();
    }
}