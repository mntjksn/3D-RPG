using UnityEngine;

public class PlayerInputController : MonoBehaviour
{
    [Header("Inventory")]
    [SerializeField] private GameObject inventoryPanel;

    [Header("Potion")]
    [SerializeField] private PotionSlotUI potionSlotUI;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.I))
            inventoryPanel.SetActive((true));

        if (Input.GetKeyDown(KeyCode.Q))
            potionSlotUI.TryUseRegisteredPotion();
    }
}