using System.Collections;
using UnityEngine;

public class HitFlash : MonoBehaviour
{
    [SerializeField] private Renderer[] renderers;
    [SerializeField] private float flashDuration = 0.15f;

    private Material[] materials;
    private Coroutine flashCoroutine;

    private void Awake()
    {
        if (renderers == null || renderers.Length == 0)
            renderers = GetComponentsInChildren<Renderer>();

        var list = new System.Collections.Generic.List<Material>();

        foreach (Renderer rend in renderers)
        {
            foreach (Material mat in rend.materials)
                list.Add(mat);
        }

        materials = list.ToArray();
        SetHitAmount(0f);
    }

    public void PlayFlash()
    {
        if (flashCoroutine != null)
            StopCoroutine(flashCoroutine);

        flashCoroutine = StartCoroutine(FlashRoutine());
    }

    private IEnumerator FlashRoutine()
    {
        float time = 0f;

        // ºü¸£°Ô »¡°³Áü
        while (time < 0.05f)
        {
            time += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, time / 0.05f);
            SetHitAmount(t);
            yield return null;
        }

        time = 0f;

        // ÃµÃµÈ÷ µ¹¾Æ¿È
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

    private void SetHitAmount(float value)
    {
        foreach (Material mat in materials)
        {
            if (mat != null && mat.HasProperty("_HitAmount"))
                mat.SetFloat("_HitAmount", value);
        }
    }
}