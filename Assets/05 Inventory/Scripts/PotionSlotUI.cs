using UnityEditorInternal.Profiling.Memory.Experimental;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PotionSlotUI : MonoBehaviour, IDropHandler
{
    [SerializeField] private Image iconImage;

    private PlayerHealth playerHealth;

    private void OnEnable()
    {
        // 동적으로 찾기
        if (playerHealth == null && PlayerManager.Instance != null)
            playerHealth = PlayerManager.Instance.Health;

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

        if (itemData != null)
            QuestService.NotifyEquipItem(itemData.itemName);

        Debug.Log($"{itemData.itemName} 포션 슬롯 등록");
        SoundManager.Instance.PlaySFX(SfxType.Equip);
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