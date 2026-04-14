using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UpgradeInventorySlotUI : MonoBehaviour,
    IBeginDragHandler, IDragHandler, IEndDragHandler,
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
        if (canvasGroup == null)
            canvasGroup = GetComponent<CanvasGroup>();
    }

    public void SetIndex(int index)
    {
        slotIndex = index;
        gameObject.name = $"UpgradeInventorySlot_{index}";
    }

    public void SetEmpty()
    {
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

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (isDragging)
            return;

        if (currentItemData == null || currentCount <= 0)
            return;

        ItemTooltipUI.Instance?.Show(currentItemData);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        ItemTooltipUI.Instance?.Hide();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (slotIndex < 0 || currentItemData == null)
            return;

        if (currentItemData.itemType != ItemType.Material)
            return;

        isDragging = true;

        ItemTooltipUI.Instance?.Hide();

        UpgradeDragData.Clear();
        UpgradeDragData.DraggedItem = currentItemData;
        UpgradeDragData.SourceSlot = this;

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
        bool wasSource = UpgradeDragData.SourceSlot == this;

        if (!isDragging && !wasSource)
            return;

        isDragging = false;

        if (canvasGroup != null)
            canvasGroup.blocksRaycasts = true;

        UpgradeDragData.Clear();
    }

    private void CreateDragIcon()
    {
        Canvas canvas = GetComponentInParent<Canvas>();
        if (canvas == null || currentItemData == null || currentItemData.icon == null)
            return;

        GameObject iconObj = new GameObject("UpgradeDragIcon");
        iconObj.transform.SetParent(canvas.transform, false);
        iconObj.transform.SetAsLastSibling();

        Image dragImage = iconObj.AddComponent<Image>();
        dragImage.sprite = currentItemData.icon;
        dragImage.raycastTarget = false;

        RectTransform rect = iconObj.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(40f, 40f);

        UpgradeDragData.DragIconObject = iconObj;
        UpgradeDragData.DragIconImage = dragImage;
    }

    private void UpdateDragIconPosition(PointerEventData eventData)
    {
        if (UpgradeDragData.DragIconObject == null)
            return;

        UpgradeDragData.DragIconObject.transform.position = eventData.position;
    }
}