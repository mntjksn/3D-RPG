using TMPro;
using UnityEngine;

public class StatBlockUI : MonoBehaviour
{
    [SerializeField] private TMP_Text mainText;
    [SerializeField] private TMP_Text subText;

    public void Set(string main, string sub)
    {
        if (mainText != null)
            mainText.text = main;
        else
            Debug.LogError("mainText ¿¬°á ¾ÈµÊ", this);

        if (subText != null)
        {
            subText.text = sub;
            subText.gameObject.SetActive(!string.IsNullOrEmpty(sub));
        }
        else
        {
            Debug.LogError("subText ¿¬°á ¾ÈµÊ", this);
        }
    }
}