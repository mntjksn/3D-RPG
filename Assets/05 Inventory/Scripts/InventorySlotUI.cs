using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// 인벤 슬롯 UI, 드래그 및 툴팁 처리 담당
public class InventorySlotUI : MonoBehaviour,
    IBeginDragHandler, IDragHandler, IEndDragHandler, IDropHandler,
    IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private Image iconImage;
    [SerializeField] private TMP_Text countText;
    [SerializeField] private CanvasGroup canvasGroup;

    private int slotIndex = -1;
    private ItemData currentItemData;
    private int currentCount;
    private bool isDragging;

    public int SlotIndex => slotIndex;
    public ItemData CurrentItemData => currentItemData;
    public int CurrentCount => currentCount;

    private void Awake()
    {
        canvasGroup ??= GetComponent<CanvasGroup>();
    }

    public void SetIndex(int index)
    {
        slotIndex = index;
        gameObject.name = $"Slot_{index}";
    }

    // 슬롯 비우기
    public void SetEmpty()
    {
        currentItemData = null;
        currentCount = 0;

        if (iconImage != null)
        {
            iconImage.sprite = null;
            iconImage.enabled = false;
        }

        countText?.SetText(string.Empty);
    }

    // 슬롯에 아이템 설정
    public void SetItem(ItemData itemData, int count)
    {
        currentItemData = itemData;
        currentCount = count;

        if (iconImage != null)
        {
            iconImage.sprite = itemData != null ? itemData.icon : null;
            iconImage.enabled = itemData != null && itemData.icon != null;
        }

        countText?.SetText(count > 1 ? count.ToString() : string.Empty);
    }

    // 마우스 오버 시 툴팁 표시
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (isDragging) return;
        if (currentItemData == null || currentCount <= 0) return;

        ItemTooltipUI.Instance?.Show(currentItemData);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        ItemTooltipUI.Instance?.Hide();
    }

    // 드래그 시작
    public void OnBeginDrag(PointerEventData eventData)
    {
        if (slotIndex < 0 || currentItemData == null) return;

        isDragging = true;

        ItemTooltipUI.Instance?.Hide();

        InventoryDragData.Clear();
        InventoryDragData.DraggedItem = currentItemData;
        InventoryDragData.SourceSlot = this;
        InventoryDragData.SourceEquipmentSlot = null;

        CreateDragIcon();
        UpdateDragIconPosition(eventData);

        if (canvasGroup != null)
            canvasGroup.blocksRaycasts = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!isDragging) return;

        UpdateDragIconPosition(eventData);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        bool wasSource = InventoryDragData.SourceSlot == this;

        if (!isDragging && !wasSource) return;

        isDragging = false;

        if (canvasGroup != null)
            canvasGroup.blocksRaycasts = true;

        InventoryDragData.Clear();
    }

    // 드롭 처리
    public void OnDrop(PointerEventData eventData)
    {
        ItemTooltipUI.Instance?.Hide();

        if (InventoryManager.Instance == null) return;

        // 인벤 → 인벤 이동
        if (InventoryDragData.SourceSlot != null)
        {
            int fromIndex = InventoryDragData.SourceSlot.SlotIndex;
            int toIndex = slotIndex;

            if (fromIndex < 0 || toIndex < 0 || fromIndex == toIndex) return;

            InventoryManager.Instance.SwapSlots(fromIndex, toIndex);
            return;
        }

        // 장비 → 인벤 (해제)
        if (InventoryDragData.SourceEquipmentSlot != null)
        {
            EquipmentManager.Instance?.UnequipItem(InventoryDragData.SourceEquipmentSlot.SlotType);
        }
    }

    // 클릭 시 장착
    public void OnClickSlot()
    {
        if (slotIndex < 0 || currentItemData == null) return;

        EquipmentManager.Instance?.EquipItemFromSlot(slotIndex);
    }

    // 드래그 아이콘 생성
    private void CreateDragIcon()
    {
        Canvas canvas = GetComponentInParent<Canvas>();
        if (canvas == null || currentItemData == null || currentItemData.icon == null) return;

        GameObject iconObj = new GameObject("DragIcon");
        iconObj.transform.SetParent(canvas.transform, false);
        iconObj.transform.SetAsLastSibling();

        Image dragImage = iconObj.AddComponent<Image>();
        dragImage.sprite = currentItemData.icon;
        dragImage.raycastTarget = false;

        RectTransform rect = iconObj.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(40f, 40f);

        InventoryDragData.DragIconObject = iconObj;
        InventoryDragData.DragIconImage = dragImage;
    }

    // 드래그 아이콘 위치 갱신
    private void UpdateDragIconPosition(PointerEventData eventData)
    {
        if (InventoryDragData.DragIconObject == null) return;

        InventoryDragData.DragIconObject.transform.position = eventData.position;
    }
}