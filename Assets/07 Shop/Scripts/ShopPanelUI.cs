using System.Collections;
using UnityEngine;
using UnityEngine.UI;

// 상점 패널 열기, 거래 팝업, UI 갱신 담당
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
        // 싱글톤 설정
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
        // PlayerStat 준비될 때까지 대기
        yield return new WaitUntil(() =>
            PlayerManager.Instance != null &&
            PlayerManager.Instance.Stat != null);

        playerStat = PlayerManager.Instance.Stat;

        if (ShopManager.Instance != null)
        {
            ShopManager.Instance.OnTradeSuccess -= RefreshAllUI;
            ShopManager.Instance.OnTradeSuccess += RefreshAllUI;
        }

        RefreshAllUI();
    }

    private void OnDisable()
    {
        if (ShopManager.Instance != null)
            ShopManager.Instance.OnTradeSuccess -= RefreshAllUI;
    }

    // 버튼 이벤트 연결
    private void BindButtons()
    {
        closeButton?.onClick.AddListener(CloseShop);
    }

    // 상점 슬롯 클릭 처리
    public void OnClickShopSlot(ShopSlotUI slotUI)
    {
        if (slotUI == null) return;

        ItemData itemData = slotUI.CurrentItemData;
        if (itemData == null) return;

        int maxBuyCount = ShopService.GetMaxBuyCount(itemData, playerStat);
        tradePopupUI?.OpenBuy(itemData, slotUI.SlotIndex, maxBuyCount);
    }

    // 인벤 슬롯 클릭 처리
    public void OnClickShopInventorySlot(ShopInventorySlotUI slotUI)
    {
        if (slotUI == null) return;

        ItemData itemData = slotUI.CurrentItemData;
        if (itemData == null) return;

        int ownedCount = slotUI.CurrentCount;
        tradePopupUI?.OpenSell(itemData, slotUI.SlotIndex, ownedCount);
    }

    // 구매 확정
    public void ConfirmBuy(int shopSlotIndex, int quantity)
    {
        bool success = ShopService.TryBuy(shopSlotIndex, quantity, shopUI, playerStat);

        if (!success)
            RefreshAllUI();
    }

    // 판매 확정
    public void ConfirmSell(int inventorySlotIndex, int quantity)
    {
        bool success = ShopService.TrySell(inventorySlotIndex, quantity, playerStat);

        if (!success)
            RefreshAllUI();
    }

    // 상점 UI 전체 갱신
    public void RefreshAllUI()
    {
        shopUI?.RefreshUI();
        shopInventoryUI?.RefreshUI();
    }

    // 상점 닫기
    public void CloseShop()
    {
        UIManager.Instance?.ClosePanel(UIPanelType.Shop);
    }
}