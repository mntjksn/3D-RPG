using UnityEngine;
using UnityEngine.Playables;

public class IntroTimelineController : MonoBehaviour
{
    [SerializeField] private PlayableDirector director;
    [SerializeField] private GameObject introVirtualCamera;
    [SerializeField] private FadeUI fadeUI;

    private void Awake()
    {
        director.stopped += OnTimelineStopped;
    }

    private void OnDestroy()
    {
        director.stopped -= OnTimelineStopped;
    }

    private void OnTimelineStopped(PlayableDirector obj)
    {
        // 1. 화면 검게
        fadeUI.FadeOut(() =>
        {
            // 2. 카메라 전환
            introVirtualCamera.SetActive(false);

            // 3. 다시 밝게
            fadeUI.FadeIn();
        });
    }
}