using TMPro;
using UnityEngine;
using UnityEngine.UI;

// 현재 진행 중 퀘스트 UI 표시 담당
public class QuestProgressUI : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject panelRoot;
    [SerializeField] private TMP_Text questNameText;
    [SerializeField] private TMP_Text questDescText;
    [SerializeField] private Slider progressSlider;
    [SerializeField] private TMP_Text progressText;

    private bool isSubscribed;

    private void Start()
    {
        RefreshUI();
    }

    private void OnEnable()
    {
        BindEvents();
        RefreshUI();
    }

    private void OnDisable()
    {
        UnbindEvents();
    }

    // 이벤트 등록
    private void BindEvents()
    {
        if (isSubscribed) return;

        var manager = QuestManager.Instance;
        if (manager == null) return;

        manager.OnQuestAccepted += RefreshUI;
        manager.OnQuestUpdated += RefreshUI;
        manager.OnQuestCompleted += RefreshUI;
        manager.OnQuestRewardClaimed += RefreshUI;

        isSubscribed = true;
    }

    // 이벤트 해제
    private void UnbindEvents()
    {
        if (!isSubscribed) return;

        var manager = QuestManager.Instance;
        if (manager == null) return;

        manager.OnQuestAccepted -= RefreshUI;
        manager.OnQuestUpdated -= RefreshUI;
        manager.OnQuestCompleted -= RefreshUI;
        manager.OnQuestRewardClaimed -= RefreshUI;

        isSubscribed = false;
    }

    // 이벤트용 오버로드
    private void RefreshUI(QuestData _)
    {
        RefreshUI();
    }

    // UI 갱신
    public void RefreshUI()
    {
        var manager = QuestManager.Instance;
        if (manager == null)
        {
            SetVisible(false);
            return;
        }

        QuestData questData = manager.GetCurrentActiveQuest();
        if (questData == null)
        {
            SetVisible(false);
            return;
        }

        QuestStateData state = manager.GetState(questData.questId);
        if (state == null)
        {
            SetVisible(false);
            return;
        }

        SetVisible(true);

        questNameText?.SetText(questData.questName);
        questDescText?.SetText(questData.description);

        float progress01 = manager.GetProgress01(questData);

        if (progressSlider != null)
        {
            progressSlider.minValue = 0f;
            progressSlider.maxValue = 1f;
            progressSlider.value = progress01;
        }

        if (progressText != null)
        {
            int current = Mathf.Min(state.currentCount, questData.targetCount);
            int target = questData.targetCount;
            progressText.SetText($"{current}/{target}");
        }
    }

    // 표시 여부 설정
    private void SetVisible(bool visible)
    {
        panelRoot?.SetActive(visible);
    }
}