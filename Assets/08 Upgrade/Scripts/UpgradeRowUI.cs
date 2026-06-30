using TMPro;
using UnityEngine;
using UnityEngine.UI;

// 업그레이드 행 UI 갱신 및 버튼 처리 담당
public class UpgradeRowUI : MonoBehaviour
{
    [Header("Info")]
    [SerializeField] private UpgradeType upgradeType;
    [SerializeField] private TMP_Text probabilityText;

    [Header("Background")]
    [SerializeField] private Image backgroundImage;

    [Header("Cost")]
    [SerializeField] private Image materialIconImage;
    [SerializeField] private TMP_Text materialCountText;
    [SerializeField] private TMP_Text goldText;

    [Header("Button")]
    [SerializeField] private Button upgradeButton;

    [Header("Owner UI")]
    [SerializeField] private UpgradeUI upgradeUI;

    public UpgradeType UpgradeType => upgradeType;

    private void Awake()
    {
        if (upgradeButton != null)
        {
            upgradeButton.onClick.RemoveAllListeners();
            upgradeButton.onClick.AddListener(OnClickUpgradeButton);
        }
    }

    // 업그레이드 행 UI 갱신
    public void Refresh(
        StatUpgradeData statData,
        int currentLevel,
        ItemData selectedMaterial,
        int selectedMaterialCount,
        int currentGold)
    {
        if (statData == null)
        {
            SetDefaultState();
            return;
        }

        if (statData.IsMaxLevel(currentLevel))
        {
            if (materialIconImage != null)
            {
                materialIconImage.sprite = null;
                materialIconImage.enabled = false;
            }

            materialCountText?.SetText("MAX");
            goldText?.SetText("MAX");

            if (upgradeButton != null)
                upgradeButton.interactable = false;

            return;
        }

        UpgradeLevelData levelData = statData.GetNextLevelData(currentLevel);
        if (levelData == null)
        {
            SetDefaultState();
            return;
        }

        if (materialIconImage != null)
        {
            materialIconImage.sprite = levelData.item != null ? levelData.item.icon : null;
            materialIconImage.enabled = levelData.item != null && levelData.item.icon != null;
        }

        if (probabilityText != null)
        {
            float percent = levelData.successChance * 100f;
            probabilityText.SetText($"{currentLevel}단계 확률 {percent}%");
        }

        bool hasCorrectMaterial =
            selectedMaterial != null &&
            levelData.item != null &&
            selectedMaterial.itemId == levelData.item.itemId;

        int maxFillCount = levelData.amount;
        int clampedMaterialCount = hasCorrectMaterial
            ? Mathf.Min(selectedMaterialCount, maxFillCount)
            : 0;

        materialCountText?.SetText($"{clampedMaterialCount}/{levelData.amount}");
        goldText?.SetText(levelData.goldCost.ToString("N0"));

        bool hasEnoughMaterial = hasCorrectMaterial && clampedMaterialCount >= levelData.amount;
        bool hasEnoughGold = currentGold >= levelData.goldCost;

        if (upgradeButton != null)
            upgradeButton.interactable = hasEnoughMaterial && hasEnoughGold;

        bool isFull = clampedMaterialCount >= levelData.amount;

        if (backgroundImage != null)
        {
            Color color = backgroundImage.color;
            color.a = isFull ? 1f : 0.3f;
            backgroundImage.color = color;
        }

        if (materialCountText != null)
        {
            Color color = materialCountText.color;
            color.a = isFull ? 1f : 0.5f;
            materialCountText.color = color;
        }

        if (goldText != null)
        {
            Color color = goldText.color;
            color.a = isFull ? 1f : 0.5f;
            goldText.color = color;
        }
    }

    // 업그레이드 버튼 클릭 처리
    private void OnClickUpgradeButton()
    {
        if (upgradeUI == null)
            return;

        upgradeUI.OnClickUpgrade(upgradeType);
    }

    // 기본 상태로 초기화
    private void SetDefaultState()
    {
        if (materialIconImage != null)
        {
            materialIconImage.sprite = null;
            materialIconImage.enabled = false;
        }

        materialCountText?.SetText("-");
        goldText?.SetText("-");

        if (upgradeButton != null)
            upgradeButton.interactable = false;
    }
}