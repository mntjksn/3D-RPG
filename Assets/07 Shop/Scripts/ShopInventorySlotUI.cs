using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShopInventorySlotUI : MonoBehaviour
{
    [SerializeField] private Image iconImage;
    [SerializeField] private TMP_Text countText;
    [SerializeField] private Button button;

    private int slotIndex = -1;
    private ItemData currentItemData;
    private int currentCount;

    public int SlotIndex => slotIndex;
    public ItemData CurrentItemData => currentItemData;
    public int CurrentCount => currentCount;

    private void Awake()
    {
        if (button != null)
            button.onClick.AddListener(OnClickSlot);
    }

    public void SetIndex(int index)
    {
        slotIndex = index;
        gameObject.name = $"ShopInventorySlot_{index}";
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

    private void OnClickSlot()
    {
        if (slotIndex < 0 || currentItemData == null)
            return;

        ShopManager.Instance?.OnClickShopInventorySlot(this);
    }
}