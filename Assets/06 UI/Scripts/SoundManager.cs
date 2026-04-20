using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance { get; private set; }

    [Header("Audio Source")]
    [SerializeField] private AudioSource bgmSource;
    [SerializeField] private AudioSource sfxSource;

    [Header("SFX List")]
    [SerializeField] private List<SfxClipData> sfxClips;

    private Dictionary<SfxType, AudioClip> sfxDict;

    private float bgmVolume = 0.5f;
    private float sfxVolume = 1f;

    public float BgmVolume => bgmVolume;
    public float SfxVolume => sfxVolume;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        InitDictionary();
        LoadVolume();
        ApplyVolume();
    }

    private void InitDictionary()
    {
        sfxDict = new Dictionary<SfxType, AudioClip>();

        foreach (var data in sfxClips)
        {
            if (data != null && data.clip != null && !sfxDict.ContainsKey(data.type))
                sfxDict.Add(data.type, data.clip);
        }
    }

    public void PlaySFX(SfxType type)
    {
        if (!sfxDict.TryGetValue(type, out AudioClip clip))
        {
            Debug.LogWarning($"SFX ¾øÀ½: {type}");
            return;
        }

        if (sfxSource == null)
            return;

        sfxSource.pitch = Random.Range(0.95f, 1.05f);
        sfxSource.PlayOneShot(clip, sfxVolume);
    }

    public void SetBgmVolume(float value)
    {
        bgmVolume = Mathf.Clamp01(value);

        if (bgmSource != null)
            bgmSource.volume = bgmVolume;

        SaveVolume();
    }

    public void SetSfxVolume(float value)
    {
        sfxVolume = Mathf.Clamp01(value);
        SaveVolume();
    }

    private void ApplyVolume()
    {
        if (bgmSource != null)
            bgmSource.volume = bgmVolume;
    }

    private void SaveVolume()
    {
        PlayerPrefs.SetFloat("BGM_VOLUME", bgmVolume);
        PlayerPrefs.SetFloat("SFX_VOLUME", sfxVolume);
        PlayerPrefs.Save();
    }

    private void LoadVolume()
    {
        bgmVolume = PlayerPrefs.GetFloat("BGM_VOLUME", 0.5f);
        sfxVolume = PlayerPrefs.GetFloat("SFX_VOLUME", 1f);
    }
}