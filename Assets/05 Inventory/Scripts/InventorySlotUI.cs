using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InventorySlotUI : MonoBehaviour
{
    [SerializeField] private Image iconImage;
    [SerializeField] private TMP_Text countText;

    private int slotIndex;

    public int SlotIndex => slotIndex;

    public void SetIndex(int index)
    {
        slotIndex = index;
        gameObject.name = $"Slot_{index}";
    }

    public void SetEmpty()
    {
        if (iconImage != null)
        {
            iconImage.sprite = null;
            iconImage.enabled = false;
        }

        if (countText != null)
            countText.text = string.Empty;
    }

    public void SetItem(Sprite icon, int count)
    {
        if (iconImage != null)
        {
            iconImage.sprite = icon;
            iconImage.enabled = icon != null;
        }

        if (countText != null)
            countText.text = count > 2 ? count.ToString() : string.Empty;
    }
}