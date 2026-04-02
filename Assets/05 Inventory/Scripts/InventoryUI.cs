using System.Collections.Generic;
using UnityEngine;

public class InventoryUI : MonoBehaviour
{
    [Header("Slot Settings")]
    [SerializeField] private InventorySlotUI slotPrefab;
    [SerializeField] private Transform slotParent;
    [SerializeField] private int slotCount = 20;

    private readonly List<InventorySlotUI> slots = new List<InventorySlotUI>();

    public IReadOnlyList<InventorySlotUI> Slots => slots;

    private void Start()
    {
        CreateSlots();
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

        for (int i = slotParent.childCount - 1; i >= 0; i--)
        {
            Destroy(slotParent.GetChild(i).gameObject);
        }
    }
}