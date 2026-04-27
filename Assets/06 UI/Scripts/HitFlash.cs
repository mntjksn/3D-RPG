using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 피격 시 머티리얼 색상 변화 연출
public class HitFlash : MonoBehaviour
{
    [SerializeField] private Renderer[] renderers;

    private Material[] materials;
    private Coroutine flashCoroutine;

    private void Awake()
    {
        // 렌더러 자동 탐색
        if (renderers == null || renderers.Length == 0)
            renderers = GetComponentsInChildren<Renderer>();

        List<Material> list = new();

        foreach (Renderer rend in renderers)
        {
            if (rend == null) continue;

            foreach (Material mat in rend.materials)
                list.Add(mat);
        }

        materials = list.ToArray();

        // 초기 상태
        SetHitAmount(0f);
    }

    // 피격 효과 실행
    public void PlayFlash()
    {
        if (flashCoroutine != null)
            StopCoroutine(flashCoroutine);

        flashCoroutine = StartCoroutine(FlashRoutine());
    }

    private IEnumerator FlashRoutine()
    {
        float time = 0f;

        // 빠르게 빨개짐
        while (time < 0.05f)
        {
            time += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, time / 0.05f);
            SetHitAmount(t);
            yield return null;
        }

        time = 0f;

        // 천천히 원래대로
        while (time < 0.2f)
        {
            time += Time.deltaTime;
            float t = 1f - Mathf.SmoothStep(0f, 1f, time / 0.2f);
            SetHitAmount(t);
            yield return null;
        }

        SetHitAmount(0f);
        flashCoroutine = null;
    }

    // 쉐이더 값 적용
    private void SetHitAmount(float value)
    {
        for (int i = 0; i < materials.Length; i++)
        {
            Material mat = materials[i];

            if (mat != null && mat.HasProperty("_HitAmount"))
                mat.SetFloat("_HitAmount", value);
        }
    }
}