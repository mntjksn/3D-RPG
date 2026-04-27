using System;

// 퀘스트 진행 상태 데이터
[Serializable]
public class QuestStateData
{
    public string questId;

    public bool isAccepted;
    public bool isCompleted;
    public bool isRewardClaimed;

    public int currentCount;
}