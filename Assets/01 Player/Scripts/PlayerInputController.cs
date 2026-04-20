using UnityEngine;

public class PlayerInputController : MonoBehaviour
{
    [Header("Potion")]
    [SerializeField] private PotionSlotUI potionSlotUI;

    private PlayerActionLock actionLock;

    private void Awake()
    {
        actionLock = GetComponent<PlayerActionLock>();
    }

    private void Update()
    {
        if (actionLock != null && !actionLock.CanUI)
            return;

        if (Input.GetKeyDown(KeyCode.I))
            UIManager.Instance.TryOpenPanel(UIPanelType.Inventory);

        if (Input.GetKeyDown(KeyCode.O))
            UIManager.Instance.TryOpenPanel(UIPanelType.PlayerInfo);

        if (Input.GetKeyDown(KeyCode.Escape))
            UIManager.Instance.TryOpenPanel(UIPanelType.Setting);

        if (Input.GetKeyDown(KeyCode.Q))
        {
            if (!UIManager.Instance.IsAnyPanelOpen)
                potionSlotUI.TryUseRegisteredPotion();
        }
    }
}