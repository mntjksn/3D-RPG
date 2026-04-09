using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ItemTooltipUI : MonoBehaviour
{
    public static ItemTooltipUI Instance { get; private set; }

    [Header("UI")]
    [SerializeField] private Image iconImage;
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text descText;

    [Header("Follow Mouse")]
    [SerializeField] private Vector2 offset = new Vector2(20f, -20f);

    private RectTransform rectTransform;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        rectTransform = GetComponent<RectTransform>();

        gameObject.SetActive(false);
    }

    private void Update()
    {
        if (!gameObject.activeSelf)
            return;

        FollowMouse();
    }

    public void Show(ItemData itemData)
    {
        if (itemData == null)
            return;

        gameObject.SetActive(true);
        transform.SetAsLastSibling();

        if (iconImage != null)
            iconImage.sprite = itemData.icon;

        if (nameText != null)
            nameText.text = itemData.itemName;

        if (descText != null)
            descText.text = itemData.description;

        FollowMouse();
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }

    private void FollowMouse()
    {
        rectTransform.position = (Vector2)Input.mousePosition + offset;
    }
}