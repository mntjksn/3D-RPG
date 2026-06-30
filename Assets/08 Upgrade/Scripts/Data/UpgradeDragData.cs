using UnityEngine;
using UnityEngine.UI;

// 업그레이드 UI 드래그 상태 공유 데이터
public static class UpgradeDragData
{
    public static ItemData DraggedItem;
    public static UpgradeInventorySlotUI SourceSlot;

    public static GameObject DragIconObject;
    public static Image DragIconImage;

    // 드래그 상태 초기화
    public static void Clear()
    {
        DraggedItem = null;
        SourceSlot = null;

        // 드래그 아이콘 제거
        if (DragIconObject != null)
        {
            Object.Destroy(DragIconObject);
            DragIconObject = null;
        }

        DragIconImage = null;
    }
}