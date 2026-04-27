using UnityEngine;
using UnityEngine.Playables;

// 인트로 타임라인 재생 및 페이드 연출 담당
public class IntroTimelineController : MonoBehaviour
{
    [SerializeField] private PlayableDirector director;
    [SerializeField] private GameObject introVirtualCamera;
    [SerializeField] private FadeUI fadeUI;

    private void Awake()
    {
        if (director == null)
            return;

        director.stopped += OnTimelineStopped;

        // 타임라인 일시정지 상태로 시작
        director.initialTime = 0;
        director.Play();
        director.Pause();
    }

    private void Start()
    {
        // 첫 페이드 인 후 타임라인 재생
        fadeUI?.FadeIn(false, () =>
        {
            director?.Resume();
        });
    }

    private void OnDestroy()
    {
        if (director != null)
            director.stopped -= OnTimelineStopped;
    }

    // 타임라인 종료 후 페이드 전환
    private void OnTimelineStopped(PlayableDirector obj)
    {
        fadeUI?.FadeOut(() =>
        {
            introVirtualCamera?.SetActive(false);
            fadeUI?.FadeIn(true);
        });
    }
}