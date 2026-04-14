using UnityEngine;
using UnityEngine.UI;

public static class UpgradeDragData
{
    public static ItemData DraggedItem;
    public static UpgradeInventorySlotUI SourceSlot;
    public static GameObject DragIconObject;
    public static Image DragIconImage;

    public static void Clear()
    {
        DraggedItem = null;
        SourceSlot = null;

        if (DragIconObject != null)
            Object.Destroy(DragIconObject);

        DragIconObject = null;
        DragIconImage = null;
    }
}