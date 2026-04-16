using TMPro;
using UnityEngine;

public class QuestMarkView : MonoBehaviour
{
    [SerializeField] private TMP_Text markText;

    public void SetState(QuestMarkState state)
    {
        if (markText == null)
            return;

        switch (state)
        {
            case QuestMarkState.None:
                markText.gameObject.SetActive(false);
                break;

            case QuestMarkState.Available:
                markText.gameObject.SetActive(true);
                markText.text = "!";
                break;

            case QuestMarkState.Progress:
                markText.gameObject.SetActive(true);
                markText.text = "...";
                break;

            case QuestMarkState.Complete:
                markText.gameObject.SetActive(true);
                markText.text = "?";
                break;
        }
    }
}