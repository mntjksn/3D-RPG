using TMPro;
using UnityEngine;

// 퀘스트 상태에 따라 NPC 머리 위 마크 표시
public class QuestMarkView : MonoBehaviour
{
    [SerializeField] private TMP_Text markText;

    // 상태에 따라 마크 변경
    public void SetState(QuestMarkState state)
    {
        if (markText == null) return;

        switch (state)
        {
            case QuestMarkState.None:
                markText.gameObject.SetActive(false);
                break;

            case QuestMarkState.Available:
                SetMark("!");
                break;

            case QuestMarkState.Progress:
                SetMark("...");
                break;

            case QuestMarkState.Complete:
                SetMark("?");
                break;
        }
    }

    // 공통 마크 처리
    private void SetMark(string text)
    {
        markText.gameObject.SetActive(true);
        markText.text = text;
    }
}