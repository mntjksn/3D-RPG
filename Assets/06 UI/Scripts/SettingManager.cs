using UnityEngine;
using UnityEngine.UI;

// 설정 UI 및 볼륨 조절 담당
public class SettingManager : MonoBehaviour
{
    [Header("UI Roots")]
    [SerializeField] private GameObject settingRoot;

    [Header("Sliders")]
    [SerializeField] private Slider bgmSlider;
    [SerializeField] private Slider sfxSlider;

    private bool isInitialized;

    private void Start()
    {
        InitSliders();
    }

    // 슬라이더 초기화 및 이벤트 연결
    private void InitSliders()
    {
        if (SoundManager.Instance == null) return;
        if (bgmSlider == null || sfxSlider == null) return;

        bgmSlider.value = SoundManager.Instance.BgmVolume;
        sfxSlider.value = SoundManager.Instance.SfxVolume;

        bgmSlider.onValueChanged.AddListener(OnBgmChanged);
        sfxSlider.onValueChanged.AddListener(OnSfxChanged);

        isInitialized = true;
    }

    // 배경음 볼륨 변경
    private void OnBgmChanged(float value)
    {
        if (!isInitialized) return;

        SoundManager.Instance?.SetBgmVolume(value);
    }

    // 효과음 볼륨 변경
    private void OnSfxChanged(float value)
    {
        if (!isInitialized) return;

        SoundManager.Instance?.SetSfxVolume(value);
    }

    // 설정창 닫기
    public void CloseSetting()
    {
        UIManager.Instance?.ClosePanel(UIPanelType.Setting);
    }

    // 게임 종료
    public void ExitGame()
    {
        if (SaveManager.Instance != null)
            _ = SaveManager.Instance.SavePlayer();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}