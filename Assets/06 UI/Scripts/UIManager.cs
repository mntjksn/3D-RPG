using System.Collections.Generic;
using UnityEngine;

// UI 패널 열기, 닫기, 입력 잠금 관리 담당
public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [System.Serializable]
    public class PanelEntry
    {
        public UIPanelType panelType;
        public GameObject panelObject;
    }

    [Header("Panels")]
    [SerializeField] private List<PanelEntry> panelEntries = new();

    private readonly Dictionary<UIPanelType, GameObject> panelDict = new();

    private UIPanelType currentOpenPanelType = UIPanelType.None;

    private PlayerActionLock playerActionLock;

    public bool IsAnyPanelOpen => currentOpenPanelType != UIPanelType.None;
    public UIPanelType CurrentOpenPanelType => currentOpenPanelType;

    private void Awake()
    {
        // 싱글톤 설정
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        InitializePanels();
    }

    private void Start()
    {
        playerActionLock = null;
    }

    // 플레이어 조작 잠금 대상 등록
    public void RegisterPlayer(PlayerActionLock actionLock)
    {
        playerActionLock = actionLock;
    }

    // 패널 목록 초기화
    private void InitializePanels()
    {
        panelDict.Clear();

        for (int i = 0; i < panelEntries.Count; i++)
        {
            PanelEntry entry = panelEntries[i];
            if (entry == null || entry.panelObject == null) continue;
            if (panelDict.ContainsKey(entry.panelType)) continue;

            panelDict.Add(entry.panelType, entry.panelObject);
            entry.panelObject.SetActive(false);
        }

        currentOpenPanelType = UIPanelType.None;
    }

    // 패널 열기 시도
    public bool TryOpenPanel(UIPanelType panelType)
    {
        if (panelType == UIPanelType.None) return false;
        if (!panelDict.TryGetValue(panelType, out GameObject targetPanel) || targetPanel == null) return false;
        if (currentOpenPanelType != UIPanelType.None && currentOpenPanelType != panelType) return false;
        if (currentOpenPanelType == panelType) return true;

        targetPanel.SetActive(true);
        currentOpenPanelType = panelType;

        Cursor.lockState = CursorLockMode.Confined;
        Cursor.visible = true;
        playerActionLock?.LockRecoverControls();

        SoundManager.Instance?.PlaySFX(SfxType.Interaction);
        return true;
    }

    // 패널 닫기
    public void ClosePanel(UIPanelType panelType)
    {
        if (panelType == UIPanelType.None) return;
        if (!panelDict.TryGetValue(panelType, out GameObject targetPanel) || targetPanel == null) return;

        targetPanel.SetActive(false);
        SoundManager.Instance?.PlaySFX(SfxType.Interaction);

        if (currentOpenPanelType == panelType)
        {
            currentOpenPanelType = UIPanelType.None;

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            playerActionLock?.UnlockRecoverControls();
        }
    }
}