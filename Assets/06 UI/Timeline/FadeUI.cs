using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// 화면 페이드 및 로딩 텍스트 처리 담당
public class FadeUI : MonoBehaviour
{
    [Header("Fade In After Activate")]
    [SerializeField] private GameObject[] activateOnFadeIn;

    [SerializeField] private GameObject fadeRoot;
    [SerializeField] private Image fadeImage;
    [SerializeField] private TMP_Text loadingText;
    [SerializeField] private float fadeDuration = 0.5f;

    private Coroutine fadeCoroutine;

    private void Awake()
    {
        // 시작 시 페이드 오브젝트 활성화
        fadeRoot?.SetActive(true);
        gameObject.SetActive(true);

        if (fadeImage != null)
        {
            Color color = fadeImage.color;
            color.a = 1f;
            fadeImage.color = color;
        }
    }

    // 화면을 어둡게 전환
    public void FadeOut(Action onComplete = null)
    {
        StartFade(0f, 1f, onComplete);
    }

    // 화면을 밝게 전환
    public void FadeIn(bool activateObjects = false, Action onComplete = null)
    {
        StartFade(1f, 0f, () =>
        {
            loadingText?.gameObject.SetActive(false);

            if (activateObjects)
            {
                foreach (GameObject obj in activateOnFadeIn)
                {
                    if (obj != null)
                        obj.SetActive(true);
                }
            }

            onComplete?.Invoke();
        });
    }

    // 페이드 시작
    private void StartFade(float start, float end, Action onComplete)
    {
        if (fadeCoroutine != null)
            StopCoroutine(fadeCoroutine);

        fadeCoroutine = StartCoroutine(FadeRoutine(start, end, onComplete));
    }

    private IEnumerator FadeRoutine(float start, float end, Action onComplete)
    {
        if (fadeImage == null)
            yield break;

        float time = 0f;

        Color fadeColor = fadeImage.color;
        Color textColor = loadingText != null ? loadingText.color : Color.white;

        float textFadeDelay = fadeDuration * 0.7f;

        while (time < fadeDuration)
        {
            time += Time.deltaTime;
            float t = Mathf.Clamp01(time / fadeDuration);

            // 화면 페이드
            fadeColor.a = Mathf.Lerp(start, end, t);
            fadeImage.color = fadeColor;

            // FadeIn일 때만 텍스트 페이드
            if (loadingText != null && end == 0f && time >= textFadeDelay)
            {
                float textT = Mathf.InverseLerp(textFadeDelay, fadeDuration, time);
                textColor.a = Mathf.Lerp(1f, 0f, textT);
                loadingText.color = textColor;
            }

            yield return null;
        }

        // 최종 값 보정
        fadeColor.a = end;
        fadeImage.color = fadeColor;

        if (loadingText != null && end == 0f)
        {
            textColor.a = 0f;
            loadingText.color = textColor;
            loadingText.gameObject.SetActive(false);
        }

        fadeCoroutine = null;
        onComplete?.Invoke();
    }
}