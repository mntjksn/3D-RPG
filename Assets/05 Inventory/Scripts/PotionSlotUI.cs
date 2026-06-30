using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// 포션 슬롯 등록 및 UI 갱신 담당
public class PotionSlotUI : MonoBehaviour, IDropHandler
{
    [SerializeField] private Image iconImage;

    private void OnEnable()
    {
        RefreshUI();
    }

    // 드롭한 아이템을 포션 슬롯에 등록
    public void OnDrop(PointerEventData eventData)
    {
        if (InventoryDragData.SourceSlot == null)
            return;

        ItemData itemData = InventoryDragData.SourceSlot.CurrentItemData;
        if (itemData == null)
            return;

        if (itemData.itemType != ItemType.Consumable)
            return;

        PotionSlotManager.Instance?.SetPotion(itemData.itemId);
        RefreshUI();

        QuestService.NotifyEquipItem(itemData.itemName);
        SoundManager.Instance?.PlaySFX(SfxType.Equip);
    }

    // 등록된 포션 ItemData 조회
    public ItemData GetRegisteredItemData()
    {
        if (PotionSlotManager.Instance == null || InventoryManager.Instance == null)
            return null;

        string itemId = PotionSlotManager.Instance.RegisteredItemId;
        if (string.IsNullOrEmpty(itemId))
            return null;

        return InventoryManager.Instance.GetItemData(itemId);
    }

    // 포션 슬롯 UI 갱신
    public void RefreshUI()
    {
        if (iconImage == null)
            return;

        ItemData itemData = GetRegisteredItemData();

        if (itemData == null)
        {
            iconImage.sprite = null;
            iconImage.enabled = false;
            return;
        }

        iconImage.sprite = itemData.icon;
        iconImage.enabled = itemData.icon != null;
    }

    // 포션 슬롯 비우기
    public void ClearSlot()
    {
        PotionSlotManager.Instance?.ClearPotion();
        RefreshUI();
    }
}