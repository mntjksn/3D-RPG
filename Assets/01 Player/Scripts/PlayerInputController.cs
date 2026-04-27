using UnityEngine;
using Photon.Pun;

// 키보드 단축키 입력 처리 - UI 패널 열기 및 포션 사용
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

        // 씬 내 Canvas에서 PotionSlotUI 탐색
        potionSlotUI = FindObjectOfType<PotionSlotUI>();
    }

    private void Update()
    {
        if (!photonView.IsMine) return;

        // 채팅 입력은 UI 잠금과 무관하게 항상 처리
        if (Input.GetKeyDown(KeyCode.Return))
        {
            ChatManager.Instance?.OnPressEnter();
            return;
        }

        if (actionLock != null && !actionLock.CanUI) return;

        if (Input.GetKeyDown(KeyCode.I)) UIManager.Instance.TryOpenPanel(UIPanelType.Inventory);
        if (Input.GetKeyDown(KeyCode.O)) UIManager.Instance.TryOpenPanel(UIPanelType.PlayerInfo);
        if (Input.GetKeyDown(KeyCode.Escape)) UIManager.Instance.TryOpenPanel(UIPanelType.Setting);

        // Q키: 패널이 닫혀 있을 때만 포션 사용
        if (Input.GetKeyDown(KeyCode.Q) && !UIManager.Instance.IsAnyPanelOpen)
            TryUsePotion();
    }

    // 포션 슬롯에 등록된 아이템을 PlayerHealth에 전달해 사용 시도
    private void TryUsePotion()
    {
        if (PotionSlotManager.Instance == null || InventoryManager.Instance == null) return;

        string itemId = PotionSlotManager.Instance.RegisteredItemId;
        if (string.IsNullOrEmpty(itemId)) return;

        ItemData potionData = InventoryManager.Instance.GetItemData(itemId);
        if (potionData == null) return;

        PlayerManager.Instance?.Health?.TryUsePotion(potionData);
    }
}