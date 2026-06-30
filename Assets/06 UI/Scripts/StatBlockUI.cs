using TMPro;
using UnityEngine;

// 스탯 블록 UI 텍스트 설정 담당
public class StatBlockUI : MonoBehaviour
{
    [SerializeField] private TMP_Text mainText;
    [SerializeField] private TMP_Text subText;

    // 메인 / 서브 텍스트 설정
    public void Set(string main, string sub)
    {
        if (mainText != null)
            mainText.SetText(main);

        if (subText != null)
        {
            subText.SetText(sub);
            subText.gameObject.SetActive(!string.IsNullOrEmpty(sub));
        }
    }
}