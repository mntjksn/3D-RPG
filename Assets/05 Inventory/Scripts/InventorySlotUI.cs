using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InventorySlotUI : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IDropHandler
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
        if (canvasGroup == null)
            canvasGroup = GetComponent<CanvasGroup>();
    }

    public void SetIndex(int index)
    {
        slotIndex = index;
        gameObject.name = $"Slot_{index}";
    }

    public void SetEmpty()
    {
        slotIndex = -1;
        currentItemData = null;
        currentCount = 0;

        if (iconImage != null)
        {
            iconImage.sprite = null;
            iconImage.enabled = false;
        }

        if (countText != null)
            countText.text = string.Empty;
    }

    public void SetItem(ItemData itemData, int count)
    {
        currentItemData = itemData;
        currentCount = count;

        if (iconImage != null)
        {
            iconImage.sprite = itemData != null ? itemData.icon : null;
            iconImage.enabled = itemData != null && itemData.icon != null;
        }

        if (countText != null)
            countText.text = count > 1 ? count.ToString() : string.Empty;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (slotIndex < 0 || currentItemData == null)
            return;

        isDragging = true;

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
        if (!isDragging)
            return;

        UpdateDragIconPosition(eventData);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        bool wasSource = InventoryDragData.SourceSlot == this;

        if (!isDragging && !wasSource)
            return;

        isDragging = false;

        if (canvasGroup != null)
            canvasGroup.blocksRaycasts = true;

        InventoryDragData.Clear();
    }

    public void OnDrop(PointerEventData eventData)
    {
        if (InventoryManager.Instance == null)
            return;

        if (InventoryDragData.SourceSlot != null)
        {
            int fromIndex = InventoryDragData.SourceSlot.SlotIndex;
            int toIndex = slotIndex;

            if (fromIndex < 0 || toIndex < 0 || fromIndex == toIndex)
                return;

            InventoryManager.Instance.SwapSlots(fromIndex, toIndex);
            return;
        }

        if (InventoryDragData.SourceEquipmentSlot != null)
        {
            EquipmentManager.Instance?.UnequipItem(InventoryDragData.SourceEquipmentSlot.SlotType);
        }
    }

    public void OnClickSlot()
    {
        if (slotIndex < 0 || currentItemData == null)
            return;

        EquipmentManager.Instance?.EquipItemFromSlot(slotIndex);
    }

    private void CreateDragIcon()
    {
        Canvas canvas = GetComponentInParent<Canvas>();
        if (canvas == null || currentItemData == null || currentItemData.icon == null)
            return;

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

    private void UpdateDragIconPosition(PointerEventData eventData)
    {
        if (InventoryDragData.DragIconObject == null)
            return;

        InventoryDragData.DragIconObject.transform.position = eventData.position;
    }
}