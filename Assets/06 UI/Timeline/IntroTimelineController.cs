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

        // 타임라인 일시정지 상태로 시작
        director.initialTime = 0;
        director.Play();
        director.Pause();
    }

    private void Start()
    {
        // 첫 번째 FadeIn은 오브젝트 활성화 없이
        fadeUI.FadeIn(false, () =>
        {
            director.Resume();
        });
    }

    private void OnDestroy()
    {
        director.stopped -= OnTimelineStopped;
    }

    private void OnTimelineStopped(PlayableDirector obj)
    {
        fadeUI.FadeOut(() =>
        {
            introVirtualCamera.SetActive(false);
            // 마지막 FadeIn은 오브젝트 활성화 포함
            fadeUI.FadeIn(true);
        });
    }
}