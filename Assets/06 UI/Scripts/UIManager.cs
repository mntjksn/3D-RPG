using System.Collections.Generic;
using UnityEngine;

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
    [SerializeField] private List<PanelEntry> panelEntries = new List<PanelEntry>();

    private readonly Dictionary<UIPanelType, GameObject> panelDict = new Dictionary<UIPanelType, GameObject>();

    private UIPanelType currentOpenPanelType = UIPanelType.None;
    private GameObject currentOpenPanelObject;

    public bool IsAnyPanelOpen => currentOpenPanelType != UIPanelType.None;
    public UIPanelType CurrentOpenPanelType => currentOpenPanelType;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        InitializePanels();
    }

    private void InitializePanels()
    {
        panelDict.Clear();

        for (int i = 0; i < panelEntries.Count; i++)
        {
            PanelEntry entry = panelEntries[i];

            if (entry == null || entry.panelObject == null)
                continue;

            if (!panelDict.ContainsKey(entry.panelType))
                panelDict.Add(entry.panelType, entry.panelObject);

            entry.panelObject.SetActive(false);
        }

        currentOpenPanelType = UIPanelType.None;
        currentOpenPanelObject = null;
    }

    public bool TryOpenPanel(UIPanelType panelType)
    {
        if (panelType == UIPanelType.None)
            return false;

        if (!panelDict.TryGetValue(panelType, out GameObject targetPanel) || targetPanel == null)
        {
            Debug.LogWarning($"UIManager: {panelType} 패널이 등록되지 않았음");
            return false;
        }

        if (currentOpenPanelType != UIPanelType.None && currentOpenPanelType != panelType)
            return false;

        if (currentOpenPanelType == panelType)
            return true;

        targetPanel.SetActive(true);
        currentOpenPanelType = panelType;
        currentOpenPanelObject = targetPanel;

        SoundManager.Instance.PlaySFX(SfxType.Interaction);
        return true;
    }

    public void ClosePanel(UIPanelType panelType)
    {
        if (panelType == UIPanelType.None)
            return;

        if (!panelDict.TryGetValue(panelType, out GameObject targetPanel) || targetPanel == null)
            return;

        SoundManager.Instance.PlaySFX(SfxType.Interaction);
        targetPanel.SetActive(false);

        if (currentOpenPanelType == panelType)
        {
            currentOpenPanelType = UIPanelType.None;
            currentOpenPanelObject = null;
        }
    }
}