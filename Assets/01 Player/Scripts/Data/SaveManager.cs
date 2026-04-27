using System.Collections;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using Firebase.Database;
using Firebase.Auth;

// Firebase 기반 세이브/로드 및 자동 저장 관리
public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance { get; private set; }

    [Header("Auto Save")]
    [SerializeField] private float autoSaveInterval = 15f;

    private float autoSaveTimer;
    private bool isDirty;              // 저장이 필요한 변경 사항 존재 여부
    private bool hasLoadedInThisScene; // 씬당 중복 로드 방지 플래그
    private bool isSaving;             // 동시 저장 방지 플래그

    private DatabaseReference dbRef;
    private Coroutine loadCoroutine;

    // 현재 로그인된 유저 UID
    private string Uid => FirebaseAuth.DefaultInstance.CurrentUser?.UserId;

    // Firebase 저장 경로
    private DatabaseReference SaveDataRef => dbRef.Child("players").Child(Uid).Child("saveData");

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

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void Start()
    {
        dbRef = FirebaseDatabase.DefaultInstance.RootReference;
    }

    private void Update()
    {
        autoSaveTimer += Time.unscaledDeltaTime;

        if (autoSaveTimer < autoSaveInterval)
            return;

        autoSaveTimer = 0f;

        if (isDirty)
            _ = SavePlayerSafe();
    }

    private void OnApplicationQuit()
    {
        if (isDirty)
            _ = SavePlayerSafe();
    }

    private void OnApplicationPause(bool pause)
    {
        if (pause && isDirty)
            _ = SavePlayerSafe();
    }

    // 씬 전환 시 호출 - Main 씬은 PlayerSpawner가 로드를 담당하므로 제외
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        hasLoadedInThisScene = false;

        if (scene.name == "Main")
            return;

        RestartLoadCoroutine(LoadWhenReady());
    }

    // 실행 중인 로드 코루틴을 중단하고 새 코루틴으로 교체
    private void RestartLoadCoroutine(IEnumerator routine)
    {
        if (loadCoroutine != null)
            StopCoroutine(loadCoroutine);

        loadCoroutine = StartCoroutine(routine);
    }

    // 필요한 매니저가 모두 준비될 때까지 대기 후 로드 (최대 5초)
    private IEnumerator LoadWhenReady()
    {
        float timer = 0f;
        float timeout = 5f;

        while (!AreManagersReady() && timer < timeout)
        {
            timer += Time.unscaledDeltaTime;
            yield return null;
        }

        if (hasLoadedInThisScene)
            yield break;

        yield return LoadPlayerCoroutine();
        hasLoadedInThisScene = true;
    }

    // 로드에 필요한 모든 매니저 인스턴스가 준비되었는지 확인
    private bool AreManagersReady()
    {
        return PlayerManager.Instance != null
            && InventoryManager.Instance != null
            && EquipmentManager.Instance != null
            && PotionSlotManager.Instance != null
            && UpgradeManager.Instance != null
            && QuestManager.Instance != null;
    }

    // 변경 사항 발생 시 외부에서 호출 - 다음 자동 저장 주기에 저장됨
    public void MarkDirty()
    {
        isDirty = true;
    }

    // 동시 저장을 방지하는 안전한 저장 래퍼
    public async Task SavePlayerSafe()
    {
        if (isSaving) return;

        isSaving = true;

        try
        {
            await SavePlayer();
        }
        finally
        {
            isSaving = false;
        }
    }

    // Firebase에 세이브 데이터를 JSON으로 직렬화하여 저장
    public async Task SavePlayer()
    {
        if (string.IsNullOrEmpty(Uid))
        {
            Debug.LogWarning("로그인 상태가 아닙니다.");
            return;
        }

        PlayerSaveData saveData = BuildSaveData();

        try
        {
            string json = JsonUtility.ToJson(saveData);
            await SaveDataRef.SetRawJsonValueAsync(json);

            isDirty = false;
            Debug.Log("Firebase 저장 완료");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Firebase 저장 실패: {e.Message}");
        }
    }

    // 각 매니저에서 세이브 데이터를 수집해 하나의 구조체로 조립
    private PlayerSaveData BuildSaveData()
    {
        PlayerSaveData saveData = new PlayerSaveData();

        if (PlayerManager.Instance != null)
        {
            PlayerSaveData pd = PlayerManager.Instance.GetSaveData();
            if (pd != null)
            {
                saveData.level = pd.level;
                saveData.currentExp = pd.currentExp;
                saveData.currentHp = pd.currentHp;
                saveData.gold = pd.gold;
            }
        }

        if (InventoryManager.Instance != null)
            saveData.inventoryItems = InventoryManager.Instance.GetSaveData();

        if (EquipmentManager.Instance != null)
            saveData.equipmentData = EquipmentManager.Instance.GetSaveData();

        if (PotionSlotManager.Instance != null)
            saveData.potionSlot = PotionSlotManager.Instance.GetSaveData();

        if (UpgradeManager.Instance != null)
            saveData.upgradeData = UpgradeManager.Instance.GetSaveData();

        if (QuestManager.Instance != null)
            saveData.questData = QuestManager.Instance.GetSaveData();

        return saveData;
    }

    // Firebase에서 데이터를 읽어 각 매니저에 적용, 실패 시 기본값으로 초기화
    private IEnumerator LoadPlayerCoroutine()
    {
        if (string.IsNullOrEmpty(Uid))
        {
            Debug.LogWarning("로그인 상태가 아닙니다.");
            InitializeAll();
            yield break;
        }

        var task = SaveDataRef.GetValueAsync();
        yield return new WaitUntil(() => task.IsCompleted);

        if (task.IsFaulted || task.IsCanceled)
        {
            Debug.LogWarning("Firebase 불러오기 실패. 기본값으로 시작합니다.");
            InitializeAll();
            isDirty = false;
            yield break;
        }

        DataSnapshot snapshot = task.Result;

        if (!snapshot.Exists)
        {
            Debug.Log("저장 데이터 없음. 기본값으로 시작합니다.");
            InitializeAll();
            isDirty = false;
            yield break;
        }

        string json = snapshot.GetRawJsonValue();
        PlayerSaveData saveData = JsonUtility.FromJson<PlayerSaveData>(json);

        if (saveData == null)
        {
            Debug.LogWarning("데이터 파싱 실패. 기본값으로 시작합니다.");
            InitializeAll();
            isDirty = false;
            yield break;
        }

        ApplySaveData(saveData);

        // 한 프레임 대기 후 UI 갱신 이벤트 강제 발행
        yield return null;

        PlayerManager.Instance?.Stat?.ForceNotify();

        isDirty = false;
        Debug.Log("Firebase 불러오기 완료");
    }

    // 세이브 데이터를 각 매니저에 순서대로 적용
    private void ApplySaveData(PlayerSaveData saveData)
    {
        InventoryManager.Instance?.LoadFromSaveData(saveData.inventoryItems);
        EquipmentManager.Instance?.LoadFromSaveData(saveData.equipmentData);
        PotionSlotManager.Instance?.LoadFromSaveData(saveData.potionSlot);
        UpgradeManager.Instance?.LoadFromSaveData(saveData.upgradeData);
        QuestManager.Instance?.LoadFromSaveData(saveData.questData);

        // 플레이어 스탯은 장비/업그레이드 적용 후 마지막에 로드
        PlayerManager.Instance?.LoadFromSaveData(saveData);
    }

    // 외부에서 수동으로 로드 시작 (예: 씬 전환 후 PlayerSpawner 호출)
    public void LoadPlayer()
    {
        RestartLoadCoroutine(LoadPlayerCoroutine());
    }

    // Firebase 저장 데이터 삭제 후 전체 초기화
    public async Task DeleteSave()
    {
        if (!string.IsNullOrEmpty(Uid))
        {
            try
            {
                await SaveDataRef.RemoveValueAsync();
                Debug.Log("저장 데이터 삭제 완료");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Firebase 삭제 실패: {e.Message}");
            }
        }

        InitializeAll();
        isDirty = false;
    }

    // 모든 매니저를 기본값으로 초기화
    private void InitializeAll()
    {
        PlayerManager.Instance?.InitializePlayer();
        InventoryManager.Instance?.InitializeInventory();
        EquipmentManager.Instance?.InitializeEquipment();
        PotionSlotManager.Instance?.InitializePotion();
        UpgradeManager.Instance?.InitializeUpgrade();
        QuestManager.Instance?.InitializeQuest();
    }
}