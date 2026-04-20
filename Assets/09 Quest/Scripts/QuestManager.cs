using System;
using System.Collections.Generic;
using UnityEngine;

public class QuestManager : MonoBehaviour
{
    public static QuestManager Instance { get; private set; }

    [Header("Quest Database")]
    [SerializeField] private List<QuestData> questDatabase = new List<QuestData>();

    public event Action<QuestData> OnQuestAccepted;
    public event Action<QuestData> OnQuestUpdated;
    public event Action<QuestData> OnQuestCompleted;
    public event Action<QuestData> OnQuestRewardClaimed;

    private readonly Dictionary<string, QuestStateData> questStates = new();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public void InitializeQuest()
    {
        questStates.Clear();
    }

    public bool HasAcceptedQuest(string questId)
    {
        if (string.IsNullOrEmpty(questId))
            return false;

        return questStates.TryGetValue(questId, out var state) && state.isAccepted;
    }

    public QuestStateData GetState(string questId)
    {
        if (string.IsNullOrEmpty(questId))
            return null;

        questStates.TryGetValue(questId, out var state);
        return state;
    }

    public QuestData GetQuestData(string questId)
    {
        if (string.IsNullOrEmpty(questId))
            return null;

        for (int i = 0; i < questDatabase.Count; i++)
        {
            QuestData questData = questDatabase[i];

            if (questData == null)
                continue;

            if (string.Equals(questData.questId, questId, StringComparison.Ordinal))
                return questData;
        }

        return null;
    }

    public List<QuestData> GetActiveQuestDatas()
    {
        List<QuestData> activeQuests = new List<QuestData>();

        foreach (var pair in questStates)
        {
            QuestStateData state = pair.Value;

            if (state == null || !state.isAccepted || state.isCompleted)
                continue;

            QuestData questData = GetQuestData(state.questId);
            if (questData == null)
                continue;

            activeQuests.Add(questData);
        }

        return activeQuests;
    }

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

        Debug.Log($"퀘스트 수락: {questData.questName}");
        SoundManager.Instance.PlaySFX(SfxType.Interaction);
        OnQuestAccepted?.Invoke(questData);

        SaveManager.Instance?.MarkDirty();
    }

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
            Debug.Log($"퀘스트 완료: {questData.questName}");
            SoundManager.Instance.PlaySFX(SfxType.Interaction);
            OnQuestCompleted?.Invoke(questData);
        }

        SaveManager.Instance?.MarkDirty();
    }

    public void MarkRewardClaimed(QuestData questData)
    {
        if (questData == null)
            return;

        if (!questStates.TryGetValue(questData.questId, out var state))
            return;

        state.isRewardClaimed = true;

        Debug.Log($"퀘스트 보상 수령: {questData.questName}");
        SoundManager.Instance.PlaySFX(SfxType.BuySell);
        OnQuestRewardClaimed?.Invoke(questData);

        SaveManager.Instance?.MarkDirty();
    }

    public QuestSaveData GetSaveData()
    {
        QuestSaveData saveData = new QuestSaveData();

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

    public void LoadFromSaveData(QuestSaveData saveData)
    {
        questStates.Clear();

        if (saveData == null || saveData.questStates == null)
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

    public QuestData GetCurrentActiveQuest()
    {
        foreach (QuestData questData in questDatabase)
        {
            if (questData == null)
                continue;

            QuestStateData state = GetState(questData.questId);
            if (state == null)
                continue;

            if (!state.isAccepted)
                continue;

            if (state.isRewardClaimed)
                continue;

            return questData;
        }

        return null;
    }

    public float GetProgress01(QuestData questData)
    {
        if (questData == null || questData.targetCount <= 0)
            return 0f;

        QuestStateData state = GetState(questData.questId);
        if (state == null)
            return 0f;

        return Mathf.Clamp01((float)state.currentCount / questData.targetCount);
    }

    public QuestData GetNextQuestInOrder()
    {
        for (int i = 0; i < questDatabase.Count; i++)
        {
            QuestData questData = questDatabase[i];

            if (questData == null)
                continue;

            QuestStateData state = GetState(questData.questId);

            if (state == null)
                return questData;

            if (!state.isAccepted)
                return questData;

            if (state.isAccepted && !state.isRewardClaimed)
                return questData;
        }

        return null;
    }

    public QuestMarkState GetNextQuestMarkState()
    {
        QuestData questData = GetNextQuestInOrder();

        if (questData == null)
            return QuestMarkState.None;

        QuestStateData state = GetState(questData.questId);

        if (state == null)
            return QuestMarkState.Available;

        if (!state.isAccepted)
            return QuestMarkState.Available;

        if (state.isAccepted && !state.isCompleted)
            return QuestMarkState.Progress;

        if (state.isCompleted && !state.isRewardClaimed)
            return QuestMarkState.Complete;

        return QuestMarkState.None;
    }
}