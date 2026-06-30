using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// 강화 재료 슬롯 드롭 및 UI 갱신 담당
public class UpgradeSelectedMaterialSlotUI : MonoBehaviour, IDropHandler
{
    [Header("Type")]
    [SerializeField] private UpgradeType upgradeType;

    [Header("UI")]
    [SerializeField] private Image iconImage;
    [SerializeField] private TMP_Text countText;

    [Header("Owner")]
    [SerializeField] private UpgradeUI upgradeUI;

    // 재료 드롭 처리
    public void OnDrop(PointerEventData eventData)
    {
        if (UpgradeDragData.DraggedItem == null)
            return;

        if (UpgradeDragData.DraggedItem.itemType != ItemType.Material)
            return;

        if (upgradeUI == null)
            return;

        upgradeUI.OnDropMaterial(upgradeType, UpgradeDragData.DraggedItem);
        SoundManager.Instance?.PlaySFX(SfxType.Equip);
    }

    // 슬롯 UI 갱신
    public void RefreshSlot(ItemData itemData, int ownedCount)
    {
        if (iconImage != null)
        {
            iconImage.sprite = itemData != null ? itemData.icon : null;
            iconImage.enabled = itemData != null && itemData.icon != null;
        }

        countText?.SetText(itemData != null ? ownedCount.ToString() : string.Empty);
    }

    // 슬롯 비우기
    public void ClearSlot()
    {
        if (iconImage != null)
        {
            iconImage.sprite = null;
            iconImage.enabled = false;
        }

        countText?.SetText(string.Empty);
    }
}