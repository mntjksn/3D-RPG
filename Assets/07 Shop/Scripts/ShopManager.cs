using UnityEngine;
using UnityEngine.UI;

public class ShopManager : MonoBehaviour
{
    public static ShopManager Instance { get; private set; }

    [Header("UI")]
    [SerializeField] private GameObject shopPanel;
    [SerializeField] private ShopUI shopUI;
    [SerializeField] private ShopInventoryUI shopInventoryUI;
    [SerializeField] private TradePopupUI tradePopupUI;
    [SerializeField] private Button closeButton;

    [Header("Player")]
    [SerializeField] private PlayerStat playerStat;

    [Header("Action Lock")]
    [SerializeField] private PlayerActionLock playerActionLock;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Start()
    {
        BindButtons();
    }

    private void OnEnable()
    {
        Cursor.lockState = CursorLockMode.Confined;
        Cursor.visible = true;

        if (playerActionLock != null)
            playerActionLock.LockRecoverControls();
    }

    private void OnDisable()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (playerActionLock != null)
            playerActionLock.UnlockRecoverControls();
    }

    private void BindButtons()
    {
        if (closeButton != null)
            closeButton.onClick.AddListener(CloseShop);
    }

    public void OnClickShopSlot(ShopSlotUI slotUI)
    {
        if (slotUI == null)
            return;

        ItemData itemData = slotUI.CurrentItemData;
        if (itemData == null)
            return;

        int maxBuyCount = GetMaxBuyCount(itemData);

        Debug.Log($"상점 슬롯 클릭: {itemData.itemName} / 최대 구매 가능 수량: {maxBuyCount}");

        if (tradePopupUI != null)
            tradePopupUI.OpenBuy(itemData, slotUI.SlotIndex, maxBuyCount);
    }

    public void OnClickShopInventorySlot(ShopInventorySlotUI slotUI)
    {
        if (slotUI == null)
            return;

        ItemData itemData = slotUI.CurrentItemData;
        if (itemData == null)
            return;

        int ownedCount = slotUI.CurrentCount;

        Debug.Log($"인벤토리 슬롯 클릭: {itemData.itemName} / 보유 수량: {ownedCount}");

        if (tradePopupUI != null)
            tradePopupUI.OpenSell(itemData, slotUI.SlotIndex, ownedCount);
    }

    public void TryBuy(int shopSlotIndex, int quantity)
    {
        if (shopUI == null)
        {
            Debug.LogWarning("ShopUI가 연결되지 않았습니다.");
            return;
        }

        if (playerStat == null)
        {
            Debug.LogWarning("PlayerStat이 연결되지 않았습니다.");
            return;
        }

        if (InventoryManager.Instance == null)
        {
            Debug.LogWarning("InventoryManager가 없습니다.");
            return;
        }

        if (quantity <= 0)
        {
            Debug.LogWarning("구매 수량이 올바르지 않습니다.");
            return;
        }

        ItemData itemData = shopUI.GetItemDataByShopIndex(shopSlotIndex);
        if (itemData == null)
        {
            Debug.LogWarning($"상점 슬롯[{shopSlotIndex}]의 아이템을 찾을 수 없습니다.");
            return;
        }

        int totalPrice = itemData.buyPrice * quantity;

        if (playerStat.Gold < totalPrice)
        {
            Debug.Log("골드가 부족합니다.");
            return;
        }

        bool added = InventoryManager.Instance.AddItem(itemData, quantity);
        if (!added)
        {
            Debug.Log("인벤토리가 가득 차서 구매할 수 없습니다.");
            return;
        }

        bool usedGold = playerStat.UseGold(totalPrice);
        if (!usedGold)
        {
            Debug.LogWarning("골드 차감 실패");

            // 골드 차감 실패 시 방금 넣은 아이템 롤백
            InventoryManager.Instance.RemoveItem(itemData.itemId, quantity);
            return;
        }

        Debug.Log($"구매 완료: {itemData.itemName} / 수량: {quantity} / 가격: {totalPrice}");
        RefreshAllUI();
    }

    public void TrySell(int inventorySlotIndex, int quantity)
    {
        if (playerStat == null)
        {
            Debug.LogWarning("PlayerStat이 연결되지 않았습니다.");
            return;
        }

        if (InventoryManager.Instance == null)
        {
            Debug.LogWarning("InventoryManager가 없습니다.");
            return;
        }

        if (quantity <= 0)
        {
            Debug.LogWarning("판매 수량이 올바르지 않습니다.");
            return;
        }

        ItemData itemData = InventoryManager.Instance.GetItemAtSlot(inventorySlotIndex);
        if (itemData == null)
        {
            Debug.LogWarning($"인벤토리 슬롯[{inventorySlotIndex}]의 아이템을 찾을 수 없습니다.");
            return;
        }

        int ownedCount = InventoryManager.Instance.GetItemCountAtSlot(inventorySlotIndex);
        if (ownedCount < quantity)
        {
            Debug.LogWarning($"판매 수량이 보유 수량보다 많습니다. 보유: {ownedCount}, 판매 요청: {quantity}");
            return;
        }

        bool removed = InventoryManager.Instance.RemoveItemAtSlot(inventorySlotIndex, quantity);
        if (!removed)
        {
            Debug.LogWarning("아이템 제거 실패");
            return;
        }

        int totalSellPrice = itemData.sellPrice * quantity;
        playerStat.AddGold(totalSellPrice);

        Debug.Log($"판매 완료: {itemData.itemName} / 수량: {quantity} / 가격: {totalSellPrice}");
        RefreshAllUI();
    }

    private int GetMaxBuyCount(ItemData itemData)
    {
        if (itemData == null || playerStat == null)
            return 1;

        if (itemData.buyPrice <= 0)
            return 999;

        int maxByGold = playerStat.Gold / itemData.buyPrice;

        // 팝업 최소값 때문에 0이면 1로 열리게 둘지, 아예 못 사게 둘지 선택
        return Mathf.Max(1, maxByGold);
    }

    public void RefreshAllUI()
    {
        if (shopUI != null)
            shopUI.RefreshUI();

        if (shopInventoryUI != null)
            shopInventoryUI.RefreshUI();
    }

    public void CloseShop()
    {
        if (shopPanel != null)
            shopPanel.SetActive(false);
    }
}