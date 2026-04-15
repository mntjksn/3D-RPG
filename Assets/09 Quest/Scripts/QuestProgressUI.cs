using TMPro;
using UnityEngine;
using UnityEngine.UI;

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
        BindEvents();
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

    private void BindEvents()
    {
        if (isSubscribed)
            return;

        if (QuestManager.Instance == null)
            return;

        QuestManager.Instance.OnQuestAccepted += RefreshUI;
        QuestManager.Instance.OnQuestUpdated += RefreshUI;
        QuestManager.Instance.OnQuestCompleted += RefreshUI;
        QuestManager.Instance.OnQuestRewardClaimed += RefreshUI;

        isSubscribed = true;
    }

    private void UnbindEvents()
    {
        if (!isSubscribed)
            return;

        if (QuestManager.Instance == null)
            return;

        QuestManager.Instance.OnQuestAccepted -= RefreshUI;
        QuestManager.Instance.OnQuestUpdated -= RefreshUI;
        QuestManager.Instance.OnQuestCompleted -= RefreshUI;
        QuestManager.Instance.OnQuestRewardClaimed -= RefreshUI;

        isSubscribed = false;
    }

    private void RefreshUI(QuestData _)
    {
        RefreshUI();
    }

    public void RefreshUI()
    {
        if (QuestManager.Instance == null)
        {
            SetVisible(false);
            return;
        }

        QuestData questData = QuestManager.Instance.GetCurrentActiveQuest();
        if (questData == null)
        {
            SetVisible(false);
            return;
        }

        QuestStateData state = QuestManager.Instance.GetState(questData.questId);
        if (state == null)
        {
            SetVisible(false);
            return;
        }

        SetVisible(true);

        if (questNameText != null)
            questNameText.text = questData.questName;

        if (questDescText != null)
            questDescText.text = questData.description;

        float progress01 = QuestManager.Instance.GetProgress01(questData);

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
            progressText.text = $"{current}/{target}";
        }
    }

    private void SetVisible(bool visible)
    {
        if (panelRoot != null)
            panelRoot.SetActive(visible);
    }
}