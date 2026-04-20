using UnityEngine;

public class IntroExplanation : MonoBehaviour
{
    [SerializeField] private GameObject explanationPanel; // º≥∏Ì√¢

    public void Open()
    {
        explanationPanel.SetActive(true);
    }

    public void Close()
    {
        explanationPanel.SetActive(false);
    }
}
