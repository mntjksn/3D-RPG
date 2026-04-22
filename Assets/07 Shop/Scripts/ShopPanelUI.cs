using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class ShopPanelUI : MonoBehaviour
{
    public static ShopPanelUI Instance { get; private set; }

    [Header("UI")]
    [SerializeField] private GameObject shopPanel;
    [SerializeField] private ShopUI shopUI;
    [SerializeField] private ShopInventoryUI shopInventoryUI;
    [SerializeField] private TradePopupUI tradePopupUI;
    [SerializeField] private Button closeButton;

    private PlayerStat playerStat;

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
        StartCoroutine(InitAndOpen());
    }

    private IEnumerator InitAndOpen()
    {
        yield return new WaitUntil(() =>
            PlayerManager.Instance != null &&
            PlayerManager.Instance.Stat != null);

        playerStat = PlayerManager.Instance.Stat;

        if (ShopManager.Instance != null)
            ShopManager.Instance.OnTradeSuccess += RefreshAllUI;

        RefreshAllUI();
    }

    private void OnDisable()
    {
        if (ShopManager.Instance != null)
            ShopManager.Instance.OnTradeSuccess -= RefreshAllUI;
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

        int maxBuyCount = ShopService.GetMaxBuyCount(itemData, playerStat);

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

    public void ConfirmBuy(int shopSlotIndex, int quantity)
    {
        bool success = ShopService.TryBuy(shopSlotIndex, quantity, shopUI, playerStat);

        if (!success)
            RefreshAllUI();
    }

    public void ConfirmSell(int inventorySlotIndex, int quantity)
    {
        bool success = ShopService.TrySell(inventorySlotIndex, quantity, playerStat);

        if (!success)
            RefreshAllUI();
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
            UIManager.Instance.ClosePanel(UIPanelType.Shop);
    }
}