using System;

[Serializable]
public class QuestStateData
{
    public string questId;
    public bool isAccepted;
    public bool isCompleted;
    public bool isRewardClaimed;
    public int currentCount;
}