using UnityEngine;

[CreateAssetMenu(fileName = "QuestData", menuName = "Quest/Quest Data")]
public class QuestData : ScriptableObject
{
    [Header("ID")]
    public string questId;

    [Header("이름")]
    public string questName;

    [Header("설명")]
    [TextArea]
    public string description;

    [Header("퀘스트 타입")]
    public QuestType questType;

    [Header("목표")]
    public string targetId;
    public int targetCount;

    [Header("보상")]
    public int rewardGold;
    public int rewardExp;
    public ItemData rewardItem;
    public int rewardItemCount;

    [Header("대화")]
    public QuestDialogueData startDialogue;
    public QuestDialogueData progressDialogue;
    public QuestDialogueData completeDialogue;
    public QuestDialogueData noQuestDialogue;

    [Header("연계")]
    public QuestData nextQuest;
}