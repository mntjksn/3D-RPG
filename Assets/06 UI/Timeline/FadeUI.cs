using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class FadeUI : MonoBehaviour
{
    [SerializeField] private GameObject fadeRoot;
    [SerializeField] private Image fadeImage;
    [SerializeField] private TMP_Text loadingText;
    [SerializeField] private float fadeDuration = 0.5f;

    private Coroutine fadeCoroutine;

    private void Awake()
    {
        if (fadeImage != null)
        {
            Color color = fadeImage.color;
            color.a = 0f;
            fadeImage.color = color;
        }
    }

    public void FadeOut(Action onComplete = null)
    {
        StartFade(0f, 1f, onComplete);
    }

    public void FadeIn(Action onComplete = null)
    {
        StartFade(1f, 0f, () =>
        {
            if (loadingText != null)
                loadingText.gameObject.SetActive(false);

            onComplete?.Invoke();
        });
    }

    private void StartFade(float start, float end, Action onComplete)
    {
        if (fadeCoroutine != null)
            StopCoroutine(fadeCoroutine);

        fadeCoroutine = StartCoroutine(FadeRoutine(start, end, onComplete));
    }

    private IEnumerator FadeRoutine(float start, float end, Action onComplete)
    {
        float time = 0f;

        Color fadeColor = fadeImage.color;
        Color textColor = loadingText != null ? loadingText.color : Color.white;

        float textFadeDelay = fadeDuration * 0.7f; // 마지막 30%에서만 텍스트 페이드

        while (time < fadeDuration)
        {
            time += Time.deltaTime;
            float t = Mathf.Clamp01(time / fadeDuration);

            // 화면 페이드 (전체 시간)
            fadeColor.a = Mathf.Lerp(start, end, t);
            fadeImage.color = fadeColor;

            // 텍스트는 마지막 구간에서만 페이드
            if (loadingText != null && end == 0f) // FadeIn일 때만 적용
            {
                if (time >= textFadeDelay)
                {
                    float textT = Mathf.InverseLerp(textFadeDelay, fadeDuration, time);
                    textColor.a = Mathf.Lerp(1f, 0f, textT);
                    loadingText.color = textColor;
                }
            }

            yield return null;
        }

        // 마무리
        fadeColor.a = end;
        fadeImage.color = fadeColor;

        if (loadingText != null && end == 0f)
        {
            textColor.a = 0f;
            loadingText.color = textColor;
            loadingText.gameObject.SetActive(false);
            fadeRoot.SetActive(false);
        }

        fadeCoroutine = null;
        onComplete?.Invoke();
    }
}