using TMPro;
using UnityEngine;
using UnityEngine.UI;

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

            if (materialCountText != null)
                materialCountText.text = "MAX";

            if (goldText != null)
                goldText.text = "MAX";

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
            probabilityText.text = $"{currentLevel}단계 확률 {percent}%";
        }

        bool hasCorrectMaterial =
            selectedMaterial != null &&
            levelData.item != null &&
            selectedMaterial.itemId == levelData.item.itemId;

        int maxFillCount = levelData.amount;

        int clampedMaterialCount = hasCorrectMaterial
            ? Mathf.Min(selectedMaterialCount, maxFillCount)
            : 0;

        if (materialCountText != null)
            materialCountText.text = $"{clampedMaterialCount}/{levelData.amount}";

        if (goldText != null)
            goldText.text = levelData.goldCost.ToString("N0");

        bool hasEnoughMaterial = hasCorrectMaterial && clampedMaterialCount >= levelData.amount;
        bool hasEnoughGold = currentGold >= levelData.goldCost;

        if (upgradeButton != null)
            upgradeButton.interactable = hasEnoughMaterial && hasEnoughGold;

        bool isFull = clampedMaterialCount >= levelData.amount;

        if (backgroundImage != null)
        {
            Color color = backgroundImage.color;
            color.a = isFull ? 1f : 0.3f; // 꽉 차면 진하게, 아니면 흐리게
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

    private void OnClickUpgradeButton()
    {
        if (upgradeUI == null)
            return;

        upgradeUI.OnClickUpgrade(upgradeType);
    }

    private void SetDefaultState()
    {
        if (materialIconImage != null)
        {
            materialIconImage.sprite = null;
            materialIconImage.enabled = false;
        }

        if (materialCountText != null)
            materialCountText.text = "-";

        if (goldText != null)
            goldText.text = "-";

        if (upgradeButton != null)
            upgradeButton.interactable = false;
    }
}