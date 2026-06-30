using UnityEngine;
using UnityEngine.UI;

// 인벤토리 드래그 상태 공유 데이터
public static class InventoryDragData
{
    public static ItemData DraggedItem;
    public static InventorySlotUI SourceSlot;
    public static EquipmentSlotUI SourceEquipmentSlot;

    public static GameObject DragIconObject;
    public static Image DragIconImage;

    // 드래그 상태 초기화
    public static void Clear()
    {
        DraggedItem = null;
        SourceSlot = null;
        SourceEquipmentSlot = null;

        // 드래그 아이콘 제거
        if (DragIconObject != null)
            Object.Destroy(DragIconObject);

        DragIconObject = null;
        DragIconImage = null;
    }
}