using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

// 장비 슬롯 UI, 드래그 및 장착 처리 담당
public class EquipmentSlotUI : MonoBehaviour, IDropHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [SerializeField] private EquipmentSlotType slotType;
    [SerializeField] private Image iconImage;
    [SerializeField] private TMP_Text infoText;
    [SerializeField] private CanvasGroup canvasGroup;

    private bool isDragging;

    public EquipmentSlotType SlotType => slotType;

    private void Awake()
    {
        canvasGroup ??= GetComponent<CanvasGroup>();
    }

    private void OnEnable()
    {
        if (EquipmentManager.Instance == null) return;

        EquipmentManager.Instance.OnEquipmentChanged += RefreshUI;
    }

    private void OnDisable()
    {
        if (EquipmentManager.Instance == null) return;

        EquipmentManager.Instance.OnEquipmentChanged -= RefreshUI;
    }

    private void Start()
    {
        RefreshUI();
    }

    // 드래그 시작
    public void OnBeginDrag(PointerEventData eventData)
    {
        if (EquipmentManager.Instance == null) return;

        ItemData equippedItem = EquipmentManager.Instance.GetEquippedItem(slotType);
        if (equippedItem == null) return;

        isDragging = true;

        InventoryDragData.Clear();
        InventoryDragData.DraggedItem = equippedItem;
        InventoryDragData.SourceSlot = null;
        InventoryDragData.SourceEquipmentSlot = this;

        CreateDragIcon(equippedItem);
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
        bool wasSource = InventoryDragData.SourceEquipmentSlot == this;

        if (!isDragging && !wasSource) return;

        isDragging = false;

        if (canvasGroup != null)
            canvasGroup.blocksRaycasts = true;

        InventoryDragData.Clear();
    }

    // 드롭 시 장착 처리
    public void OnDrop(PointerEventData eventData)
    {
        if (EquipmentManager.Instance == null) return;

        ItemData draggedItem = InventoryDragData.DraggedItem;
        InventorySlotUI sourceSlot = InventoryDragData.SourceSlot;

        if (draggedItem == null || sourceSlot == null) return;
        if (sourceSlot.SlotIndex < 0) return;
        if (!EquipmentManager.Instance.CanEquipToSlot(draggedItem, slotType)) return;

        EquipmentManager.Instance.EquipItemFromSlot(sourceSlot.SlotIndex);
    }

    // UI 갱신
    public void RefreshUI()
    {
        ItemData equippedItem = EquipmentManager.Instance?.GetEquippedItem(slotType);

        if (iconImage != null)
        {
            if (equippedItem == null)
            {
                iconImage.sprite = null;
                iconImage.enabled = false;
            }
            else
            {
                iconImage.sprite = equippedItem.icon;
                iconImage.enabled = equippedItem.icon != null;
            }
        }

        UpdateInfoText(equippedItem);
    }

    // 장비 정보 텍스트 갱신
    private void UpdateInfoText(ItemData equippedItem)
    {
        if (infoText == null) return;

        switch (slotType)
        {
            case EquipmentSlotType.Weapon:
                infoText.SetText($"공격력 + {GetAttackValue(equippedItem)}");
                break;

            case EquipmentSlotType.Armor:
                infoText.SetText($"체력 + {GetHpValue(equippedItem):N0}");
                break;

            case EquipmentSlotType.Shield:
                infoText.SetText($"방어력 + {GetDefensePercentValue(equippedItem)}%");
                break;

            case EquipmentSlotType.Shoes:
                infoText.SetText($"속도 + {GetSpeedValue(equippedItem)}");
                break;

            default:
                infoText.SetText(string.Empty);
                break;
        }
    }

    // 드래그 아이콘 생성
    private void CreateDragIcon(ItemData itemData)
    {
        if (itemData == null || itemData.icon == null) return;

        Canvas canvas = GetComponentInParent<Canvas>();
        if (canvas == null) return;

        GameObject iconObj = new GameObject("DragIcon");
        iconObj.transform.SetParent(canvas.transform, false);
        iconObj.transform.SetAsLastSibling();

        Image dragImage = iconObj.AddComponent<Image>();
        dragImage.sprite = itemData.icon;
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

    private int GetAttackValue(ItemData itemData) => itemData != null ? itemData.attackPower : 0;
    private int GetHpValue(ItemData itemData) => itemData != null ? itemData.maxHpBonus : 0;
    private int GetDefensePercentValue(ItemData itemData) => itemData != null ? itemData.shieldPower : 0;
    private float GetSpeedValue(ItemData itemData) => itemData != null ? itemData.moveSpeedBonus : 0f;
}