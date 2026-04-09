using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PotionSlotUI : MonoBehaviour, IDropHandler
{
    [SerializeField] private Image iconImage;
    [SerializeField] private PlayerHealth playerHealth;

    private void OnEnable()
    {
        RefreshUI();
    }

    public void OnDrop(PointerEventData eventData)
    {
        if (InventoryDragData.SourceSlot == null)
            return;

        ItemData itemData = InventoryDragData.SourceSlot.CurrentItemData;
        if (itemData == null)
            return;

        if (itemData.itemType != ItemType.Consumable)
        {
            Debug.Log("포션 슬롯에는 소모 아이템만 등록할 수 있습니다.");
            return;
        }

        PotionSlotManager.Instance?.SetPotion(itemData.itemId);
        RefreshUI();

        Debug.Log($"{itemData.itemName} 포션 슬롯 등록");
    }

    public ItemData GetRegisteredItemData()
    {
        if (PotionSlotManager.Instance == null || InventoryManager.Instance == null)
            return null;

        string itemId = PotionSlotManager.Instance.RegisteredItemId;
        if (string.IsNullOrEmpty(itemId))
            return null;

        return InventoryManager.Instance.GetItemData(itemId);
    }

    public bool TryUseRegisteredPotion()
    {
        if (playerHealth == null || InventoryManager.Instance == null)
            return false;

        ItemData potionData = GetRegisteredItemData();
        if (potionData == null)
            return false;

        bool used = playerHealth.TryUsePotion(potionData);
        if (!used)
            return false;

        int remaining = InventoryManager.Instance.GetItemCount(potionData.itemId);
        if (remaining <= 0)
        {
            ClearSlot();
            return true;
        }

        RefreshUI();
        return true;
    }

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

    public void ClearSlot()
    {
        PotionSlotManager.Instance?.ClearPotion();
        RefreshUI();
    }
}