using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TopPanelUI : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private TMP_Text levelText;
    [SerializeField] private TMP_Text expText;
    [SerializeField] private Slider expSlider;
    [SerializeField] private TMP_Text goldText;

    private PlayerStat playerStat;

    private void Start()
    {
        BindPlayerStat();
        RefreshNow();
    }

    private void OnEnable()
    {
        BindPlayerStat();
        Subscribe();
        RefreshNow();
    }

    private void OnDisable()
    {
        Unsubscribe();
    }

    private void BindPlayerStat()
    {
        if (PlayerManager.Instance != null)
            playerStat = PlayerManager.Instance.Stat;
    }

    private void Subscribe()
    {
        if (playerStat == null)
            return;

        playerStat.OnLevelChanged -= UpdateLevelUI;
        playerStat.OnExpChanged -= UpdateExpUI;
        playerStat.OnGoldChanged -= UpdateGoldUI;

        playerStat.OnLevelChanged += UpdateLevelUI;
        playerStat.OnExpChanged += UpdateExpUI;
        playerStat.OnGoldChanged += UpdateGoldUI;
    }

    private void Unsubscribe()
    {
        if (playerStat == null)
            return;

        playerStat.OnLevelChanged -= UpdateLevelUI;
        playerStat.OnExpChanged -= UpdateExpUI;
        playerStat.OnGoldChanged -= UpdateGoldUI;
    }

    public void RefreshNow()
    {
        if (playerStat == null)
            return;

        UpdateLevelUI(playerStat.Level);
        UpdateExpUI(playerStat.CurrentExp, playerStat.GetExpToNextLevel());
        UpdateGoldUI(playerStat.Gold);
    }

    private void UpdateLevelUI(int level)
    {
        levelText.text = $"{level}";
    }

    private void UpdateExpUI(int currentExp, int maxExp)
    {
        if (maxExp <= 0)
        {
            expSlider.maxValue = 1f;
            expSlider.value = 0f;
            expText.text = "0%";
            return;
        }

        float percent = (float)currentExp / maxExp;

        expSlider.maxValue = 1f;
        expSlider.value = percent;
        expText.text = $"{Mathf.RoundToInt(percent * 100f)}%";
    }

    private void UpdateGoldUI(int gold)
    {
        goldText.text = gold.ToString("N0");
    }
}