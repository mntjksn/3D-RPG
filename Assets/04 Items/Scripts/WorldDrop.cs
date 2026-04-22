using Photon.Pun;
using UnityEngine;

public class WorldDrop : MonoBehaviourPun
{
    [Header("Visual")]
    [SerializeField] private SpriteRenderer iconRenderer;

    private ItemData itemData;
    private string itemId;
    private int amount;
    private int goldAmount;

    private int allowedActorNumber = -1;
    private bool isPicked;

    private Camera mainCamera;

    private void Start()
    {
        mainCamera = Camera.main;
    }

    private void LateUpdate()
    {
        if (mainCamera == null)
            mainCamera = Camera.main;

        if (mainCamera == null || iconRenderer == null)
            return;

        iconRenderer.transform.rotation = mainCamera.transform.rotation;
    }

    // 마스터가 생성 직후 호출
    public void SetupGold(int gold, int ownerActorNumber)
    {
        if (!PhotonNetwork.IsMasterClient)
            return;

        photonView.RPC(nameof(RPC_SetupGold), RpcTarget.AllBuffered, gold, ownerActorNumber);
    }

    // 마스터가 생성 직후 호출
    public void SetupItem(ItemData item, int itemAmount, int ownerActorNumber)
    {
        if (!PhotonNetwork.IsMasterClient)
            return;

        if (item == null || itemAmount <= 0)
            return;

        photonView.RPC(nameof(RPC_SetupItem), RpcTarget.AllBuffered, item.itemId, itemAmount, ownerActorNumber);
    }

    [PunRPC]
    private void RPC_SetupGold(int gold, int ownerActorNumber)
    {
        goldAmount = gold;
        itemData = null;
        itemId = null;
        amount = 0;
        allowedActorNumber = ownerActorNumber;
    }

    [PunRPC]
    private void RPC_SetupItem(string newItemId, int itemAmount, int ownerActorNumber)
    {
        itemId = newItemId;
        amount = itemAmount;
        goldAmount = 0;
        allowedActorNumber = ownerActorNumber;

        // 여기 함수명은 네 프로젝트에 맞게 맞춰줘
        // 예: InventoryManager.Instance.GetItemData(itemId)
        itemData = InventoryManager.Instance != null
            ? InventoryManager.Instance.GetItemData(itemId)
            : null;

        if (iconRenderer != null && itemData != null)
            iconRenderer.sprite = itemData.icon;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!PhotonNetwork.IsMasterClient)
            return;

        if (isPicked)
            return;

        if (!other.CompareTag("Player"))
            return;

        PhotonView playerView = other.GetComponent<PhotonView>();
        if (playerView == null)
            return;

        int actorNumber = playerView.OwnerActorNr;

        // 잡은 사람만 획득 가능
        if (actorNumber != allowedActorNumber)
            return;

        isPicked = true;

        photonView.RPC(nameof(RPC_Pickup), RpcTarget.All, actorNumber);
    }

    [PunRPC]
    private void RPC_Pickup(int actorNumber)
    {
        // 먹을 수 있는 플레이어 본인 클라에서만 지급
        if (PhotonNetwork.LocalPlayer.ActorNumber == actorNumber)
        {
            if (goldAmount > 0)
            {
                PlayerManager.Instance?.AddGold(goldAmount);
                SoundManager.Instance?.PlaySFX(SfxType.ItemPickup);
            }
            else if (!string.IsNullOrEmpty(itemId))
            {
                if (itemData == null && InventoryManager.Instance != null)
                    itemData = InventoryManager.Instance.GetItemData(itemId);

                if (itemData != null)
                {
                    Debug.Log($"아이템 획득: {itemData.itemName} x{amount}");
                    InventoryManager.Instance?.AddItem(itemData, amount);
                    QuestService.NotifyCollectItem(itemData.itemName, amount);
                    SoundManager.Instance?.PlaySFX(SfxType.ItemPickup);
                }
            }
        }

        if (PhotonNetwork.IsMasterClient)
            PhotonNetwork.Destroy(gameObject);
    }
}