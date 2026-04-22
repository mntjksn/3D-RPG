using UnityEngine;
using Photon.Pun;

public class PlayerInputController : MonoBehaviourPun
{
    private PotionSlotUI potionSlotUI;
    private PlayerActionLock actionLock;

    private void Awake()
    {
        actionLock = GetComponent<PlayerActionLock>();
    }

    private void Start()
    {
        if (!photonView.IsMine) return;

        // Canvas에서 찾기
        potionSlotUI = FindObjectOfType<PotionSlotUI>();
    }

    private void Update()
    {
        if (!photonView.IsMine) return;

        // 엔터 키 처리 (채팅)
        if (Input.GetKeyDown(KeyCode.Return))
        {
            ChatManager.Instance?.OnPressEnter();
            return;
        }

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
                TryUsePotion();
        }
    }

private void TryUsePotion()
    {
        if (PotionSlotManager.Instance == null || InventoryManager.Instance == null)
            return;

        string itemId = PotionSlotManager.Instance.RegisteredItemId;
        if (string.IsNullOrEmpty(itemId))
            return;

        ItemData potionData = InventoryManager.Instance.GetItemData(itemId);
        if (potionData == null)
            return;

        PlayerHealth health = PlayerManager.Instance?.Health;
        if (health == null)
            return;

        health.TryUsePotion(potionData);
    }
}
