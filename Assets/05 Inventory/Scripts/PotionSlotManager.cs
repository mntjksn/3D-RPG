using UnityEngine;

// 포션 슬롯 등록 및 저장 데이터 관리
public class PotionSlotManager : MonoBehaviour
{
    public static PotionSlotManager Instance { get; private set; }

    private string registeredItemId;

    public string RegisteredItemId => registeredItemId;
    public bool HasPotion => !string.IsNullOrEmpty(registeredItemId);

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
    }

    // 포션 슬롯 초기화
    public void InitializePotion()
    {
        registeredItemId = string.Empty;
    }

    // 포션 등록
    public void SetPotion(string itemId)
    {
        if (string.IsNullOrEmpty(itemId))
            return;

        registeredItemId = itemId;
        SaveManager.Instance?.MarkDirty();
    }

    // 포션 제거
    public void ClearPotion()
    {
        registeredItemId = string.Empty;
        SaveManager.Instance?.MarkDirty();
    }

    // 저장 데이터 생성
    public PotionSlotSaveData GetSaveData()
    {
        return new PotionSlotSaveData
        {
            registeredItemId = registeredItemId
        };
    }

    // 저장 데이터 로드
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