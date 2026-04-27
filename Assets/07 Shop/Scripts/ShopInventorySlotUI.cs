using TMPro;
using UnityEngine;
using UnityEngine.UI;

// 상점 인벤 슬롯 UI 및 클릭 처리 담당
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
        button?.onClick.AddListener(OnClickSlot);
    }

    // 슬롯 인덱스 설정
    public void SetIndex(int index)
    {
        slotIndex = index;
        gameObject.name = $"ShopInventorySlot_{index}";
    }

    // 슬롯 비우기
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

        countText?.SetText(string.Empty);
    }

    // 슬롯 아이템 설정
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

    // 슬롯 클릭 처리
    private void OnClickSlot()
    {
        if (slotIndex < 0 || currentItemData == null)
            return;

        ShopPanelUI.Instance?.OnClickShopInventorySlot(this);
    }
}