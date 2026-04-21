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
        // 씬 로드 직후 검은화면 → 밝아지면 타임라인 재생
        fadeUI.FadeIn(() =>
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