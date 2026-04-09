using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

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
        if (button != null)
            button.onClick.AddListener(OnClickSlot);
    }

    public void SetIndex(int index)
    {
        slotIndex = index;
        gameObject.name = $"ShopSlot_{index}";
    }

    public void SetEmpty()
    {
        currentItemData = null;

        if (iconImage != null)
        {
            iconImage.sprite = null;
            iconImage.enabled = false;
        }

        if (countText != null)
            countText.text = string.Empty;
    }

    public void SetItem(ItemData itemData)
    {
        currentItemData = itemData;

        if (iconImage != null)
        {
            iconImage.sprite = itemData != null ? itemData.icon : null;
            iconImage.enabled = itemData != null && itemData.icon != null;
        }

        if (countText != null)
            countText.text = string.Empty;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (currentItemData == null)
            return;

        ItemTooltipUI.Instance?.Show(currentItemData);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        ItemTooltipUI.Instance?.Hide();
    }

    private void OnClickSlot()
    {
        if (slotIndex < 0 || currentItemData == null)
            return;

        ShopManager.Instance?.OnClickShopSlot(this);
    }
}