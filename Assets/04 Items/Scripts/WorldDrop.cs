using UnityEngine;
using static UnityEngine.Rendering.PostProcessing.SubpixelMorphologicalAntialiasing;

public class WorldDrop : MonoBehaviour
{
    [Header("Visual")]
    [SerializeField] private SpriteRenderer iconRenderer;

    private ItemData itemData;
    private int amount;
    private int goldAmount;

    private Camera mainCamera;

    private void Start()
    {
        mainCamera = Camera.main;
    }

    public void SetupGold(int gold)
    {
        goldAmount = gold;
        itemData = null;
        amount = 0;
    }

    public void SetupItem(ItemData item, int itemAmount)
    {
        itemData = item;
        amount = itemAmount;
        goldAmount = 0;

        if (iconRenderer != null && itemData != null)
            iconRenderer.sprite = itemData.icon;
    }

    private void LateUpdate()
    {
        if (mainCamera == null || iconRenderer == null)
            return;

        iconRenderer.transform.rotation = mainCamera.transform.rotation;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        Pickup();
    }

    private void Pickup()
    {
        if (goldAmount > 0)
        {
            PlayerManager.Instance.AddGold(goldAmount);
        }
        else if (itemData != null)
        {
            Debug.Log($"æ∆¿Ã≈€ »πµÊ: {itemData.itemName} x{amount}");
            InventoryManager.Instance.AddItem(itemData, amount);

            if (itemData != null)
                QuestService.NotifyCollectItem(itemData.itemName, amount);
        }

        Destroy(gameObject);
    }
}