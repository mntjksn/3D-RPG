using Photon.Pun;
using UnityEngine;

// 월드 드랍 설정, 회전 처리, 획득 처리 담당
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

    // 골드 드랍 정보 설정
    public void SetupGold(int gold, int ownerActorNumber)
    {
        if (!PhotonNetwork.IsMasterClient) return;

        photonView.RPC(nameof(RPC_SetupGold), RpcTarget.AllBuffered, gold, ownerActorNumber);
    }

    // 아이템 드랍 정보 설정
    public void SetupItem(ItemData item, int itemAmount, int ownerActorNumber)
    {
        if (!PhotonNetwork.IsMasterClient) return;
        if (item == null || itemAmount <= 0) return;

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

        itemData = InventoryManager.Instance != null
            ? InventoryManager.Instance.GetItemData(itemId)
            : null;

        if (iconRenderer != null && itemData != null)
            iconRenderer.sprite = itemData.icon;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!PhotonNetwork.IsMasterClient) return;
        if (isPicked) return;
        if (!other.CompareTag("Player")) return;

        PhotonView playerView = other.GetComponent<PhotonView>();
        if (playerView == null) return;

        int actorNumber = playerView.OwnerActorNr;

        // 지정된 플레이어만 획득 가능
        if (actorNumber != allowedActorNumber) return;

        isPicked = true;
        photonView.RPC(nameof(RPC_Pickup), RpcTarget.All, actorNumber);
    }

    [PunRPC]
    private void RPC_Pickup(int actorNumber)
    {
        // 대상 플레이어 본인 클라에서만 보상 지급
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