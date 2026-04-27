using UnityEngine;

// 퀘스트 상태 변화에 따라 NPC 마크 갱신
public class QuestNpcMarker : MonoBehaviour
{
    [SerializeField] private QuestMarkView questMarkView;

    private void Start()
    {
        // 초기 상태 갱신
        RefreshMark();

        if (QuestManager.Instance == null) return;

        QuestManager.Instance.OnQuestAccepted += HandleQuestChanged;
        QuestManager.Instance.OnQuestUpdated += HandleQuestChanged;
        QuestManager.Instance.OnQuestCompleted += HandleQuestChanged;
        QuestManager.Instance.OnQuestRewardClaimed += HandleQuestChanged;
    }

    private void OnDestroy()
    {
        if (QuestManager.Instance == null) return;

        QuestManager.Instance.OnQuestAccepted -= HandleQuestChanged;
        QuestManager.Instance.OnQuestUpdated -= HandleQuestChanged;
        QuestManager.Instance.OnQuestCompleted -= HandleQuestChanged;
        QuestManager.Instance.OnQuestRewardClaimed -= HandleQuestChanged;
    }

    // 퀘스트 상태 변경 시 마크 갱신
    private void HandleQuestChanged(QuestData questData)
    {
        RefreshMark();
    }

    // 현재 퀘스트 상태에 맞게 마크 업데이트
    public void RefreshMark()
    {
        if (questMarkView == null) return;
        if (QuestManager.Instance == null) return;

        QuestMarkState state = QuestManager.Instance.GetNextQuestMarkState();
        questMarkView.SetState(state);
    }
}