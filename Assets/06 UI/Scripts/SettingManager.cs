using UnityEngine;
using UnityEngine.UI;

public class SettingManager : MonoBehaviour
{
    [Header("UI Roots")]
    [SerializeField] private GameObject settingRoot;
    [SerializeField] private GameObject explanationRoot;

    [Header("Sliders")]
    [SerializeField] private Slider bgmSlider;
    [SerializeField] private Slider sfxSlider;

    private bool isInitialized;

    private void Start()
    {
        InitSliders();
    }

    private void InitSliders()
    {
        if (SoundManager.Instance == null)
            return;

        // 현재 볼륨값 가져오기
        bgmSlider.value = SoundManager.Instance.BgmVolume;
        sfxSlider.value = SoundManager.Instance.SfxVolume;

        // 이벤트 연결
        bgmSlider.onValueChanged.AddListener(OnBgmChanged);
        sfxSlider.onValueChanged.AddListener(OnSfxChanged);

        isInitialized = true;
    }

    private void OnBgmChanged(float value)
    {
        if (!isInitialized) return;

        SoundManager.Instance.SetBgmVolume(value);
    }

    private void OnSfxChanged(float value)
    {
        if (!isInitialized) return;

        SoundManager.Instance.SetSfxVolume(value);
    }

    public void CloseSetting()
    {
        if (settingRoot != null)
            UIManager.Instance.ClosePanel(UIPanelType.Setting);
    }

    public void ExitGame()
    {
        if (SaveManager.Instance != null)
            SaveManager.Instance.SavePlayer();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}