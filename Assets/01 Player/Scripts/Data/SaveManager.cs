using System.Collections;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using Firebase.Database;
using Firebase.Auth;

public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance { get; private set; }

    [Header("Auto Save")]
    [SerializeField] private float autoSaveInterval = 15f;

    private float autoSaveTimer;
    private bool isDirty;
    private bool hasLoadedInThisScene;
    private bool isSaving;

    private DatabaseReference dbRef;
    private Coroutine loadCoroutine;

    private string Uid => FirebaseAuth.DefaultInstance.CurrentUser?.UserId;
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

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        hasLoadedInThisScene = false;

        // Main 씬은 PlayerSpawner가 직접 로드 관리
        if (scene.name == "Main")
            return;

        RestartLoadCoroutine(LoadWhenReady());
    }

    private void RestartLoadCoroutine(IEnumerator routine)
    {
        if (loadCoroutine != null)
            StopCoroutine(loadCoroutine);

        loadCoroutine = StartCoroutine(routine);
    }

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

    private bool AreManagersReady()
    {
        return PlayerManager.Instance != null
            && InventoryManager.Instance != null
            && EquipmentManager.Instance != null
            && PotionSlotManager.Instance != null
            && UpgradeManager.Instance != null
            && QuestManager.Instance != null;
    }

    public void MarkDirty()
    {
        isDirty = true;
    }

    public async Task SavePlayerSafe()
    {
        if (isSaving)
            return;

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

    private PlayerSaveData BuildSaveData()
    {
        PlayerSaveData saveData = new PlayerSaveData();

        if (PlayerManager.Instance != null)
        {
            PlayerSaveData playerData = PlayerManager.Instance.GetSaveData();
            if (playerData != null)
            {
                saveData.level = playerData.level;
                saveData.currentExp = playerData.currentExp;
                saveData.currentHp = playerData.currentHp;
                saveData.gold = playerData.gold;
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

        yield return null;

        if (PlayerManager.Instance?.Stat != null)
            PlayerManager.Instance.Stat.ForceNotify();

        isDirty = false;
        Debug.Log("Firebase 불러오기 완료");
    }

    private void ApplySaveData(PlayerSaveData saveData)
    {
        if (InventoryManager.Instance != null)
            InventoryManager.Instance.LoadFromSaveData(saveData.inventoryItems);

        if (EquipmentManager.Instance != null)
            EquipmentManager.Instance.LoadFromSaveData(saveData.equipmentData);

        if (PotionSlotManager.Instance != null)
            PotionSlotManager.Instance.LoadFromSaveData(saveData.potionSlot);

        if (UpgradeManager.Instance != null)
            UpgradeManager.Instance.LoadFromSaveData(saveData.upgradeData);

        if (QuestManager.Instance != null)
            QuestManager.Instance.LoadFromSaveData(saveData.questData);

        if (PlayerManager.Instance != null)
            PlayerManager.Instance.LoadFromSaveData(saveData);
    }

    public void LoadPlayer()
    {
        RestartLoadCoroutine(LoadPlayerCoroutine());
    }

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

    private void InitializeAll()
    {
        if (PlayerManager.Instance != null)
            PlayerManager.Instance.InitializePlayer();

        if (InventoryManager.Instance != null)
            InventoryManager.Instance.InitializeInventory();

        if (EquipmentManager.Instance != null)
            EquipmentManager.Instance.InitializeEquipment();

        if (PotionSlotManager.Instance != null)
            PotionSlotManager.Instance.InitializePotion();

        if (UpgradeManager.Instance != null)
            UpgradeManager.Instance.InitializeUpgrade();

        if (QuestManager.Instance != null)
            QuestManager.Instance.InitializeQuest();
    }
}