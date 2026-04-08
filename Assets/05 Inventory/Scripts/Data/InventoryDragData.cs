using UnityEngine;
using UnityEngine.UI;

public static class InventoryDragData
{
    public static ItemData DraggedItem;
    public static InventorySlotUI SourceSlot;
    public static EquipmentSlotUI SourceEquipmentSlot;

    public static GameObject DragIconObject;
    public static Image DragIconImage;

    public static void Clear()
    {
        DraggedItem = null;
        SourceSlot = null;
        SourceEquipmentSlot = null;

        if (DragIconObject != null)
            Object.Destroy(DragIconObject);

        DragIconObject = null;
        DragIconImage = null;
    }
}