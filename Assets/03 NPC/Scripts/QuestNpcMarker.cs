using UnityEngine;

public class QuestNpcMarker : MonoBehaviour
{
    [SerializeField] private QuestMarkView questMarkView;

    private void Start()
    {
        RefreshMark();

        if (QuestManager.Instance != null)
        {
            QuestManager.Instance.OnQuestAccepted += HandleQuestChanged;
            QuestManager.Instance.OnQuestUpdated += HandleQuestChanged;
            QuestManager.Instance.OnQuestCompleted += HandleQuestChanged;
            QuestManager.Instance.OnQuestRewardClaimed += HandleQuestChanged;
        }
    }

    private void OnDestroy()
    {
        if (QuestManager.Instance != null)
        {
            QuestManager.Instance.OnQuestAccepted -= HandleQuestChanged;
            QuestManager.Instance.OnQuestUpdated -= HandleQuestChanged;
            QuestManager.Instance.OnQuestCompleted -= HandleQuestChanged;
            QuestManager.Instance.OnQuestRewardClaimed -= HandleQuestChanged;
        }
    }

    private void HandleQuestChanged(QuestData questData)
    {
        RefreshMark();
    }

    public void RefreshMark()
    {
        if (questMarkView == null || QuestManager.Instance == null)
            return;

        QuestMarkState state = QuestManager.Instance.GetNextQuestMarkState();
        questMarkView.SetState(state);
    }
}