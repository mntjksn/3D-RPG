using TMPro;
using UnityEngine;
using UnityEngine.UI;

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
        if (confirmButton != null)
            confirmButton.onClick.AddListener(OnClickConfirm);

        if (cancelButton != null)
            cancelButton.onClick.AddListener(OnClickCancel);

        if (quantityInputField != null)
            quantityInputField.onValueChanged.AddListener(OnValueChangedQuantity);
    }

    public void OpenBuy(ItemData itemData, int slotIndex, int maxBuyQuantity)
    {
        if (itemData == null)
            return;

        currentTradeType = TradeType.Buy;
        currentItemData = itemData;
        currentSlotIndex = slotIndex;
        maxQuantity = Mathf.Max(1, maxBuyQuantity);

        RefreshStaticUI();
        SetQuantity(1);

        gameObject.SetActive(true);
    }

    public void OpenSell(ItemData itemData, int slotIndex, int maxSellQuantity)
    {
        if (itemData == null)
            return;

        currentTradeType = TradeType.Sell;
        currentItemData = itemData;
        currentSlotIndex = slotIndex;
        maxQuantity = Mathf.Max(1, maxSellQuantity);

        RefreshStaticUI();
        SetQuantity(1);

        gameObject.SetActive(true);
    }

    private void RefreshStaticUI()
    {
        if (currentItemData == null)
            return;

        if (itemIcon != null)
        {
            itemIcon.sprite = currentItemData.icon;
            itemIcon.enabled = currentItemData.icon != null;
        }

        if (titleText != null)
        {
            titleText.text = currentTradeType == TradeType.Buy
                ? "구매하시겠습니까?"
                : "판매하시겠습니까?";
        }

        if (nameText != null)
        {
            nameText.text = currentItemData.itemName;
            nameText.enabled = nameText.text != null;
        }

        RefreshPriceText();
    }

    private void SetQuantity(int quantity)
    {
        quantity = Mathf.Clamp(quantity, 1, maxQuantity);

        if (quantityInputField != null)
            quantityInputField.SetTextWithoutNotify(quantity.ToString());

        RefreshPriceText();
    }

    public void OnValueChangedQuantity(string value)
    {
        ClampQuantity();
        RefreshPriceText();
    }

    private int GetQuantity()
    {
        if (quantityInputField == null)
            return 1;

        int quantity;
        if (!int.TryParse(quantityInputField.text, out quantity))
            quantity = 1;

        return Mathf.Clamp(quantity, 1, maxQuantity);
    }

    private void ClampQuantity()
    {
        if (quantityInputField == null)
            return;

        int quantity = GetQuantity();
        quantityInputField.SetTextWithoutNotify(quantity.ToString());
    }

    private void RefreshPriceText()
    {
        if (currentItemData == null || priceText == null)
            return;

        int quantity = GetQuantity();
        int unitPrice = currentTradeType == TradeType.Buy
            ? currentItemData.buyPrice
            : currentItemData.sellPrice;

        int totalPrice = unitPrice * quantity;

        priceText.text = currentTradeType == TradeType.Buy
            ? $"구매할 가격 : {totalPrice:N0}"
            : $"판매할 가격 : {totalPrice:N0}";
    }

    public void OnClickConfirm()
    {
        if (currentItemData == null)
            return;

        int quantity = GetQuantity();

        if (currentTradeType == TradeType.Buy)
            ShopPanelUI.Instance?.ConfirmBuy(currentSlotIndex, quantity);
        else
            ShopPanelUI.Instance?.ConfirmSell(currentSlotIndex, quantity);
        Close();
    }

    public void OnClickCancel()
    {
        Close();
    }

    public void Close()
    {
        currentItemData = null;
        currentSlotIndex = -1;
        maxQuantity = 1;

        if (quantityInputField != null)
            quantityInputField.SetTextWithoutNotify("1");

        gameObject.SetActive(false);
    }
}