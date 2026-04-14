using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UpgradeSelectedMaterialSlotUI : MonoBehaviour, IDropHandler
{
    [Header("Type")]
    [SerializeField] private UpgradeType upgradeType;

    [Header("UI")]
    [SerializeField] private Image iconImage;
    [SerializeField] private TMP_Text countText;

    [Header("Owner")]
    [SerializeField] private UpgradeUI upgradeUI;

    public void OnDrop(PointerEventData eventData)
    {
        if (UpgradeDragData.DraggedItem == null)
            return;

        if (UpgradeDragData.DraggedItem.itemType != ItemType.Material)
            return;

        // 타입별로 넣기
        if (upgradeUI != null)
        {
            upgradeUI.OnDropMaterial(upgradeType, UpgradeDragData.DraggedItem);
        }
    }

    public void RefreshSlot(ItemData itemData, int ownedCount)
    {
        if (iconImage != null)
        {
            iconImage.sprite = itemData != null ? itemData.icon : null;
            iconImage.enabled = itemData != null && itemData.icon != null;
        }

        if (countText != null)
            countText.text = itemData != null ? ownedCount.ToString() : string.Empty;
    }

    public void ClearSlot()
    {
        if (iconImage != null)
        {
            iconImage.sprite = null;
            iconImage.enabled = false;
        }

        if (countText != null)
            countText.text = string.Empty;
    }
}