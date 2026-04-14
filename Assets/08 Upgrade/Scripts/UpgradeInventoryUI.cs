using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UpgradeInventoryUI : MonoBehaviour
{
    [Header("Slot Settings")]
    [SerializeField] private UpgradeInventorySlotUI slotPrefab;
    [SerializeField] private Transform slotParent;
    [SerializeField] private int slotCount = 20;

    private readonly List<UpgradeInventorySlotUI> slots = new List<UpgradeInventorySlotUI>();
    private bool isInitialized;

    public IReadOnlyList<UpgradeInventorySlotUI> Slots => slots;

    private void Start()
    {
        CreateSlots();
        isInitialized = true;
        RefreshUI();
    }

    private void OnEnable()
    {
        if (InventoryManager.Instance != null)
            InventoryManager.Instance.OnInventoryChanged += RefreshUI;

        RefreshUI();

        Cursor.lockState = CursorLockMode.Confined;
        Cursor.visible = true;
    }

    private void OnDisable()
    {
        if (InventoryManager.Instance != null)
            InventoryManager.Instance.OnInventoryChanged -= RefreshUI;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void CreateSlots()
    {
        ClearSlots();

        for (int i = 0; i < slotCount; i++)
        {
            UpgradeInventorySlotUI slot = Instantiate(slotPrefab, slotParent);
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

            if (!IsMaterial(itemData))
                continue;

            if (displayIndex >= slots.Count)
                break;

            slots[displayIndex].SetIndex(realIndex);
            slots[displayIndex].SetItem(itemData, slotData.amount);
            displayIndex++;
        }
    }

    private bool IsMaterial(ItemData itemData)
    {
        return itemData.itemType == ItemType.Material;
    }
}