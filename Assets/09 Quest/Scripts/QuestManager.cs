using System;
using System.Collections.Generic;
using UnityEngine;

// 퀘스트 상태 관리 및 진행 처리 담당
public class QuestManager : MonoBehaviour
{
    public static QuestManager Instance { get; private set; }

    [Header("Quest Database")]
    [SerializeField] private List<QuestData> questDatabase = new();

    public event Action<QuestData> OnQuestAccepted;
    public event Action<QuestData> OnQuestUpdated;
    public event Action<QuestData> OnQuestCompleted;
    public event Action<QuestData> OnQuestRewardClaimed;

    private readonly Dictionary<string, QuestStateData> questStates = new();

    private void Awake()
    {
        // 싱글톤 설정
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    // 초기화
    public void InitializeQuest()
    {
        questStates.Clear();
    }

    // 퀘스트 수락 여부 확인
    public bool HasAcceptedQuest(string questId)
    {
        if (string.IsNullOrEmpty(questId))
            return false;

        return questStates.TryGetValue(questId, out var state) && state.isAccepted;
    }

    // 상태 데이터 조회
    public QuestStateData GetState(string questId)
    {
        if (string.IsNullOrEmpty(questId))
            return null;

        questStates.TryGetValue(questId, out var state);
        return state;
    }

    // 퀘스트 데이터 조회
    public QuestData GetQuestData(string questId)
    {
        if (string.IsNullOrEmpty(questId))
            return null;

        for (int i = 0; i < questDatabase.Count; i++)
        {
            QuestData questData = questDatabase[i];
            if (questData == null) continue;

            if (string.Equals(questData.questId, questId, StringComparison.Ordinal))
                return questData;
        }

        return null;
    }

    // 진행 중 퀘스트 목록
    public List<QuestData> GetActiveQuestDatas()
    {
        List<QuestData> result = new();

        foreach (var pair in questStates)
        {
            QuestStateData state = pair.Value;

            if (state == null || !state.isAccepted || state.isCompleted)
                continue;

            QuestData questData = GetQuestData(state.questId);
            if (questData == null)
                continue;

            result.Add(questData);
        }

        return result;
    }

    // 퀘스트 수락
    public void AcceptQuest(QuestData questData)
    {
        if (questData == null || string.IsNullOrEmpty(questData.questId))
            return;

        if (questStates.TryGetValue(questData.questId, out var existing) && existing.isAccepted)
            return;

        QuestStateData state = new QuestStateData
        {
            questId = questData.questId,
            isAccepted = true,
            isCompleted = false,
            isRewardClaimed = false,
            currentCount = 0
        };

        questStates[questData.questId] = state;

        SoundManager.Instance?.PlaySFX(SfxType.Interaction);
        OnQuestAccepted?.Invoke(questData);

        SaveManager.Instance?.MarkDirty();
    }

    // 진행도 설정
    public void SetProgress(QuestData questData, int value)
    {
        if (questData == null)
            return;

        if (!questStates.TryGetValue(questData.questId, out var state))
            return;

        if (!state.isAccepted || state.isCompleted)
            return;

        state.currentCount = Mathf.Clamp(value, 0, questData.targetCount);

        OnQuestUpdated?.Invoke(questData);

        if (state.currentCount >= questData.targetCount)
        {
            state.isCompleted = true;

            SoundManager.Instance?.PlaySFX(SfxType.Interaction);
            OnQuestCompleted?.Invoke(questData);
        }

        SaveManager.Instance?.MarkDirty();
    }

    // 보상 수령 처리
    public void MarkRewardClaimed(QuestData questData)
    {
        if (questData == null)
            return;

        if (!questStates.TryGetValue(questData.questId, out var state))
            return;

        state.isRewardClaimed = true;

        SoundManager.Instance?.PlaySFX(SfxType.BuySell);
        OnQuestRewardClaimed?.Invoke(questData);

        SaveManager.Instance?.MarkDirty();
    }

    // 저장 데이터 생성
    public QuestSaveData GetSaveData()
    {
        QuestSaveData saveData = new();

        foreach (var pair in questStates)
        {
            QuestStateData state = pair.Value;

            saveData.questStates.Add(new QuestStateData
            {
                questId = state.questId,
                isAccepted = state.isAccepted,
                isCompleted = state.isCompleted,
                isRewardClaimed = state.isRewardClaimed,
                currentCount = state.currentCount
            });
        }

        return saveData;
    }

    // 저장 데이터 로드
    public void LoadFromSaveData(QuestSaveData saveData)
    {
        questStates.Clear();

        if (saveData?.questStates == null)
            return;

        foreach (QuestStateData state in saveData.questStates)
        {
            if (state == null || string.IsNullOrEmpty(state.questId))
                continue;

            questStates[state.questId] = new QuestStateData
            {
                questId = state.questId,
                isAccepted = state.isAccepted,
                isCompleted = state.isCompleted,
                isRewardClaimed = state.isRewardClaimed,
                currentCount = state.currentCount
            };
        }
    }

    // 현재 활성 퀘스트 반환
    public QuestData GetCurrentActiveQuest()
    {
        foreach (QuestData questData in questDatabase)
        {
            if (questData == null) continue;

            QuestStateData state = GetState(questData.questId);
            if (state == null) continue;

            if (!state.isAccepted) continue;
            if (state.isRewardClaimed) continue;

            return questData;
        }

        return null;
    }

    // 진행률 (0~1)
    public float GetProgress01(QuestData questData)
    {
        if (questData == null || questData.targetCount <= 0)
            return 0f;

        QuestStateData state = GetState(questData.questId);
        if (state == null)
            return 0f;

        return Mathf.Clamp01((float)state.currentCount / questData.targetCount);
    }

    // 다음 진행 퀘스트
    public QuestData GetNextQuestInOrder()
    {
        for (int i = 0; i < questDatabase.Count; i++)
        {
            QuestData questData = questDatabase[i];
            if (questData == null) continue;

            QuestStateData state = GetState(questData.questId);

            if (state == null) return questData;
            if (!state.isAccepted) return questData;
            if (!state.isRewardClaimed) return questData;
        }

        return null;
    }

    // NPC 머리 위 마크 상태 반환
    public QuestMarkState GetNextQuestMarkState()
    {
        QuestData questData = GetNextQuestInOrder();
        if (questData == null)
            return QuestMarkState.None;

        QuestStateData state = GetState(questData.questId);

        if (state == null) return QuestMarkState.Available;
        if (!state.isAccepted) return QuestMarkState.Available;
        if (!state.isCompleted) return QuestMarkState.Progress;
        if (!state.isRewardClaimed) return QuestMarkState.Complete;

        return QuestMarkState.None;
    }
}