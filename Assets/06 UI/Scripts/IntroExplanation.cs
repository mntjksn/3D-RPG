using UnityEngine;

// 인트로 설명창 열기 / 닫기 담당
public class IntroExplanation : MonoBehaviour
{
    [SerializeField] private GameObject explanationPanel;

    // 설명창 열기
    public void Open()
    {
        explanationPanel?.SetActive(true);
    }

    // 설명창 닫기
    public void Close()
    {
        explanationPanel?.SetActive(false);
    }
}