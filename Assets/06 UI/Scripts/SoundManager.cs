using System.Collections.Generic;
using UnityEngine;

// 사운드 재생 및 볼륨 저장 담당
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
        // 싱글톤 설정
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

    // 효과음 테이블 생성
    private void InitDictionary()
    {
        sfxDict = new Dictionary<SfxType, AudioClip>();

        foreach (SfxClipData data in sfxClips)
        {
            if (data == null || data.clip == null) continue;
            if (sfxDict.ContainsKey(data.type)) continue;

            sfxDict.Add(data.type, data.clip);
        }
    }

    // 효과음 재생
    public void PlaySFX(SfxType type)
    {
        if (sfxSource == null) return;
        if (!sfxDict.TryGetValue(type, out AudioClip clip)) return;

        sfxSource.pitch = Random.Range(0.95f, 1.05f);
        sfxSource.PlayOneShot(clip, sfxVolume);
    }

    // 배경음 볼륨 변경
    public void SetBgmVolume(float value)
    {
        bgmVolume = Mathf.Clamp01(value);

        if (bgmSource != null)
            bgmSource.volume = bgmVolume;

        SaveVolume();
    }

    // 효과음 볼륨 변경
    public void SetSfxVolume(float value)
    {
        sfxVolume = Mathf.Clamp01(value);
        SaveVolume();
    }

    // 현재 볼륨 적용
    private void ApplyVolume()
    {
        if (bgmSource != null)
            bgmSource.volume = bgmVolume;
    }

    // 볼륨 저장
    private void SaveVolume()
    {
        PlayerPrefs.SetFloat("BGM_VOLUME", bgmVolume);
        PlayerPrefs.SetFloat("SFX_VOLUME", sfxVolume);
        PlayerPrefs.Save();
    }

    // 저장된 볼륨 불러오기
    private void LoadVolume()
    {
        bgmVolume = PlayerPrefs.GetFloat("BGM_VOLUME", 0.5f);
        sfxVolume = PlayerPrefs.GetFloat("SFX_VOLUME", 1f);
    }
}