using TMPro;
using UnityEngine;
using UnityEngine.UI;

// 상단 패널 레벨, 경험치, 골드 표시 담당
public class TopPanelUI : MonoBehaviour
{
    [SerializeField] private TMP_Text levelText;
    [SerializeField] private TMP_Text expPercentText;
    [SerializeField] private Slider expSlider;
    [SerializeField] private TMP_Text goldText;

    private PlayerStat playerStat;

    private void OnEnable()
    {
        Bind();
        Subscribe();
        RefreshNow();
    }

    private void OnDisable()
    {
        Unsubscribe();
    }

    // PlayerStat 연결
    private void Bind()
    {
        if (PlayerManager.Instance != null)
            playerStat = PlayerManager.Instance.Stat;
    }

    // 이벤트 등록
    private void Subscribe()
    {
        if (playerStat == null) return;

        Unsubscribe();

        playerStat.OnLevelChanged += UpdateLevelUI;
        playerStat.OnExpChanged += UpdateExpUI;
        playerStat.OnGoldChanged += UpdateGoldUI;
    }

    // 이벤트 해제
    private void Unsubscribe()
    {
        if (playerStat == null) return;

        playerStat.OnLevelChanged -= UpdateLevelUI;
        playerStat.OnExpChanged -= UpdateExpUI;
        playerStat.OnGoldChanged -= UpdateGoldUI;
    }

    // 현재 값 바로 갱신
    private void RefreshNow()
    {
        if (playerStat == null) return;

        UpdateLevelUI(playerStat.Level);
        UpdateExpUI(playerStat.CurrentExp, playerStat.GetExpToNextLevel());
        UpdateGoldUI(playerStat.Gold);
    }

    // 레벨 UI 갱신
    private void UpdateLevelUI(int level)
    {
        levelText?.SetText(level.ToString());
    }

    // 경험치 UI 갱신
    private void UpdateExpUI(int currentExp, int maxExp)
    {
        float percent = maxExp > 0 ? (float)currentExp / maxExp : 0f;

        if (expSlider != null)
        {
            expSlider.maxValue = 1f;
            expSlider.value = percent;
        }

        expPercentText?.SetText($"{Mathf.RoundToInt(percent * 100f)}%");
    }

    // 골드 UI 갱신
    private void UpdateGoldUI(int gold)
    {
        goldText?.SetText(gold.ToString("N0"));
    }
}