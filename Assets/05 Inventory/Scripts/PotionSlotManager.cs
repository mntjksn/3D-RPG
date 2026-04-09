using UnityEngine;

public class PotionSlotManager : MonoBehaviour
{
    public static PotionSlotManager Instance { get; private set; }

    private string registeredItemId;

    public string RegisteredItemId => registeredItemId;
    public bool HasPotion => !string.IsNullOrEmpty(registeredItemId);

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void InitializePotion()
    {
        registeredItemId = string.Empty;
    }

    public void SetPotion(string itemId)
    {
        registeredItemId = itemId;

        if (SaveManager.Instance != null)
            SaveManager.Instance.MarkDirty();
    }

    public void ClearPotion()
    {
        registeredItemId = string.Empty;

        if (SaveManager.Instance != null)
            SaveManager.Instance.MarkDirty();
    }

    public PotionSlotSaveData GetSaveData()
    {
        return new PotionSlotSaveData
        {
            registeredItemId = registeredItemId
        };
    }

    public void LoadFromSaveData(PotionSlotSaveData saveData)
    {
        if (saveData == null)
        {
            InitializePotion();
            return;
        }

        registeredItemId = saveData.registeredItemId;
    }
}