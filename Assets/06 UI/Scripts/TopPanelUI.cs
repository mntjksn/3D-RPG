using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TopPanelUI : MonoBehaviour
{
    [SerializeField] private TMP_Text levelText;
    [SerializeField] private TMP_Text expPercentText;
    [SerializeField] private Slider expSlider;
    [SerializeField] private TMP_Text goldText;

    private PlayerStat playerStat;

    private void Start()
    {
        Bind();
        Subscribe();
    }

    private void OnEnable()
    {
        Bind();
        Subscribe();
        RefreshNow(); // 패널 열릴 때마다 갱신
    }

    private void OnDisable()
    {
        Unsubscribe();
    }

    private void Bind()
    {
        if (PlayerManager.Instance != null)
            playerStat = PlayerManager.Instance.Stat;
    }

    private void Subscribe()
    {
        if (playerStat == null)
            return;

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

    private void RefreshNow()
    {
        if (playerStat == null)
            return;

        UpdateLevelUI(playerStat.Level);
        UpdateExpUI(playerStat.CurrentExp, playerStat.GetExpToNextLevel());
        UpdateGoldUI(playerStat.Gold);
    }

    private void UpdateLevelUI(int level)
    {
        levelText.text = level.ToString();
    }

    private void UpdateExpUI(int currentExp, int maxExp)
    {
        float percent = maxExp > 0 ? (float)currentExp / maxExp : 0f;

        expSlider.maxValue = 1f;
        expSlider.value = percent;
        expPercentText.text = $"{Mathf.RoundToInt(percent * 100f)}%";
    }

    private void UpdateGoldUI(int gold)
    {
        goldText.text = gold.ToString("N0");
    }
}