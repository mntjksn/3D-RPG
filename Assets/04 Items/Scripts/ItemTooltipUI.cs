using TMPro;
using UnityEngine;
using UnityEngine.UI;

// 아이템 툴팁 표시 및 마우스 위치 추적
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
        // 싱글톤 설정
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        rectTransform = GetComponent<RectTransform>();

        // 시작 시 숨김
        gameObject.SetActive(false);
    }

    private void Update()
    {
        if (!gameObject.activeSelf) return;

        FollowMouse();
    }

    // 툴팁 표시
    public void Show(ItemData itemData)
    {
        if (itemData == null) return;

        gameObject.SetActive(true);
        transform.SetAsLastSibling();

        if (iconImage != null)
            iconImage.sprite = itemData.icon;

        nameText?.SetText(itemData.itemName);
        descText?.SetText(itemData.description);

        FollowMouse();
    }

    // 툴팁 숨김
    public void Hide()
    {
        gameObject.SetActive(false);
    }

    // 마우스 위치 따라가기
    private void FollowMouse()
    {
        rectTransform.position = (Vector2)Input.mousePosition + offset;
    }
}