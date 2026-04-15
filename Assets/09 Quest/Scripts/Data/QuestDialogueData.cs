using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "QuestDialogueData", menuName = "Quest/Dialogue Data")]
public class QuestDialogueData : ScriptableObject
{
    [Header("ID")]
    public string dialogueId;

    [Header("대사 목록")]
    [TextArea(2, 5)]
    public List<string> lines = new List<string>();
}