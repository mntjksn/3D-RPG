using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShopSlotUI : MonoBehaviour
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
        slotIndex = -1;
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

    private void OnClickSlot()
    {
        if (slotIndex < 0 || currentItemData == null)
            return;

        ShopManager.Instance?.OnClickShopSlot(this);
    }
}