using System;
using System.Collections.Generic;

// 퀘스트 저장 데이터
[Serializable]
public class QuestSaveData
{
    public List<QuestStateData> questStates = new();
}