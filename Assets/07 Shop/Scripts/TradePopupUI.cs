using TMPro;
using UnityEngine;
using UnityEngine.UI;

// 거래 팝업 표시, 수량 입력, 구매/판매 확정 담당
public class TradePopupUI : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Image itemIcon;
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text priceText;
    [SerializeField] private TMP_InputField quantityInputField;
    [SerializeField] private Button confirmButton;
    [SerializeField] private Button cancelButton;

    private TradeType currentTradeType;
    private ItemData currentItemData;
    private int currentSlotIndex = -1;
    private int maxQuantity = 1;

    private void Awake()
    {
        confirmButton?.onClick.AddListener(OnClickConfirm);
        cancelButton?.onClick.AddListener(OnClickCancel);
        quantityInputField?.onValueChanged.AddListener(OnValueChangedQuantity);
    }

    // 구매 팝업 열기
    public void OpenBuy(ItemData itemData, int slotIndex, int maxBuyQuantity)
    {
        if (itemData == null) return;

        currentTradeType = TradeType.Buy;
        currentItemData = itemData;
        currentSlotIndex = slotIndex;
        maxQuantity = Mathf.Max(1, maxBuyQuantity);

        RefreshStaticUI();
        SetQuantity(1);

        gameObject.SetActive(true);
    }

    // 판매 팝업 열기
    public void OpenSell(ItemData itemData, int slotIndex, int maxSellQuantity)
    {
        if (itemData == null) return;

        currentTradeType = TradeType.Sell;
        currentItemData = itemData;
        currentSlotIndex = slotIndex;
        maxQuantity = Mathf.Max(1, maxSellQuantity);

        RefreshStaticUI();
        SetQuantity(1);

        gameObject.SetActive(true);
    }

    // 고정 UI 갱신
    private void RefreshStaticUI()
    {
        if (currentItemData == null) return;

        if (itemIcon != null)
        {
            itemIcon.sprite = currentItemData.icon;
            itemIcon.enabled = currentItemData.icon != null;
        }

        titleText?.SetText(currentTradeType == TradeType.Buy
            ? "구매하시겠습니까?"
            : "판매하시겠습니까?");

        if (nameText != null)
        {
            nameText.SetText(currentItemData.itemName);
            nameText.enabled = !string.IsNullOrEmpty(currentItemData.itemName);
        }

        RefreshPriceText();
    }

    // 수량 설정
    private void SetQuantity(int quantity)
    {
        quantity = Mathf.Clamp(quantity, 1, maxQuantity);
        quantityInputField?.SetTextWithoutNotify(quantity.ToString());

        RefreshPriceText();
    }

    // 수량 입력 변경 처리
    public void OnValueChangedQuantity(string value)
    {
        ClampQuantity();
        RefreshPriceText();
    }

    // 현재 수량 반환
    private int GetQuantity()
    {
        if (quantityInputField == null) return 1;

        if (!int.TryParse(quantityInputField.text, out int quantity))
            quantity = 1;

        return Mathf.Clamp(quantity, 1, maxQuantity);
    }

    // 수량 범위 보정
    private void ClampQuantity()
    {
        if (quantityInputField == null) return;

        int quantity = GetQuantity();
        quantityInputField.SetTextWithoutNotify(quantity.ToString());
    }

    // 가격 텍스트 갱신
    private void RefreshPriceText()
    {
        if (currentItemData == null || priceText == null) return;

        int quantity = GetQuantity();
        int unitPrice = currentTradeType == TradeType.Buy
            ? currentItemData.buyPrice
            : currentItemData.sellPrice;

        int totalPrice = unitPrice * quantity;

        priceText.SetText(currentTradeType == TradeType.Buy
            ? $"구매할 가격 : {totalPrice:N0}"
            : $"판매할 가격 : {totalPrice:N0}");
    }

    // 거래 확정
    public void OnClickConfirm()
    {
        if (currentItemData == null) return;

        int quantity = GetQuantity();

        if (currentTradeType == TradeType.Buy)
            ShopPanelUI.Instance?.ConfirmBuy(currentSlotIndex, quantity);
        else
            ShopPanelUI.Instance?.ConfirmSell(currentSlotIndex, quantity);

        Close();
    }

    // 거래 취소
    public void OnClickCancel()
    {
        Close();
    }

    // 팝업 닫기
    public void Close()
    {
        currentItemData = null;
        currentSlotIndex = -1;
        maxQuantity = 1;

        quantityInputField?.SetTextWithoutNotify("1");
        gameObject.SetActive(false);
    }
}