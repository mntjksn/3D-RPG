using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

// 상점 슬롯 UI, 클릭 및 툴팁 처리 담당
public class ShopSlotUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private Image iconImage;
    [SerializeField] private TMP_Text countText;
    [SerializeField] private Button button;

    private int slotIndex = -1;
    private ItemData currentItemData;

    public int SlotIndex => slotIndex;
    public ItemData CurrentItemData => currentItemData;

    private void Awake()
    {
        button?.onClick.AddListener(OnClickSlot);
    }

    // 슬롯 인덱스 설정
    public void SetIndex(int index)
    {
        slotIndex = index;
        gameObject.name = $"ShopSlot_{index}";
    }

    // 슬롯 비우기
    public void SetEmpty()
    {
        currentItemData = null;

        if (iconImage != null)
        {
            iconImage.sprite = null;
            iconImage.enabled = false;
        }

        countText?.SetText(string.Empty);
    }

    // 슬롯 아이템 설정
    public void SetItem(ItemData itemData)
    {
        currentItemData = itemData;

        if (iconImage != null)
        {
            iconImage.sprite = itemData != null ? itemData.icon : null;
            iconImage.enabled = itemData != null && itemData.icon != null;
        }

        countText?.SetText(string.Empty);
    }

    // 마우스 오버 시 툴팁 표시
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (currentItemData == null) return;

        ItemTooltipUI.Instance?.Show(currentItemData);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        ItemTooltipUI.Instance?.Hide();
    }

    // 슬롯 클릭 처리
    private void OnClickSlot()
    {
        if (slotIndex < 0 || currentItemData == null)
            return;

        ShopPanelUI.Instance?.OnClickShopSlot(this);
    }
}