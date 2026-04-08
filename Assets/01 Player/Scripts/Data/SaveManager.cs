using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance { get; private set; }

    private string SavePath => Path.Combine(Application.persistentDataPath, "player_save.json");

    [Header("Auto Save")]
    [SerializeField] private float autoSaveInterval = 15f;

    private float autoSaveTimer = 0f;
    private bool isDirty = false;
    private bool hasLoadedInThisScene = false;

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
        StartCoroutine(LoadWhenReady());
    }

    private void Update()
    {
        autoSaveTimer += Time.unscaledDeltaTime;

        if (autoSaveTimer >= autoSaveInterval)
        {
            autoSaveTimer = 0f;

            if (isDirty)
                SavePlayer();
        }
    }

    private void OnApplicationQuit()
    {
        if (isDirty)
            SavePlayer();
    }

    private void OnApplicationPause(bool pause)
    {
        if (pause && isDirty)
            SavePlayer();
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        hasLoadedInThisScene = false;
        StartCoroutine(LoadWhenReady());
    }

    private IEnumerator LoadWhenReady()
    {
        float timer = 0f;
        float timeout = 5f;

        while ((PlayerManager.Instance == null || InventoryManager.Instance == null) && timer < timeout)
        {
            timer += Time.unscaledDeltaTime;
            yield return null;
        }

        if (hasLoadedInThisScene)
            yield break;

        LoadPlayer();
        hasLoadedInThisScene = true;
    }

    public void MarkDirty()
    {
        isDirty = true;
    }

    public void SavePlayer()
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
        {
            saveData.inventoryItems = InventoryManager.Instance.GetSaveData();
        }

        if (EquipmentManager.Instance != null)
        {
            saveData.equipmentData = EquipmentManager.Instance.GetSaveData();
        }

        string json = JsonUtility.ToJson(saveData, true);
        File.WriteAllText(SavePath, json);

        isDirty = false;

        Debug.Log($"저장 완료: {SavePath}");
    }

    public void LoadPlayer()
    {
        if (!File.Exists(SavePath))
        {
            Debug.Log("저장 파일이 없습니다. 기본값으로 시작합니다.");

            if (PlayerManager.Instance != null)
                PlayerManager.Instance.InitializePlayer();

            if (InventoryManager.Instance != null)
                InventoryManager.Instance.InitializeInventory();

            if (EquipmentManager.Instance != null)
                EquipmentManager.Instance.InitializeEquipment();

            isDirty = false;
            return;
        }

        string json = File.ReadAllText(SavePath);
        PlayerSaveData saveData = JsonUtility.FromJson<PlayerSaveData>(json);

        if (saveData == null)
        {
            Debug.LogWarning("저장 데이터를 불러오지 못했습니다.");
            return;
        }

        if (PlayerManager.Instance != null)
            PlayerManager.Instance.LoadFromSaveData(saveData);

        if (InventoryManager.Instance != null)
            InventoryManager.Instance.LoadFromSaveData(saveData.inventoryItems);

        if (EquipmentManager.Instance != null)
            EquipmentManager.Instance.LoadFromSaveData(saveData.equipmentData);

        isDirty = false;

        Debug.Log("불러오기 완료");
    }

    public void DeleteSave()
    {
        if (File.Exists(SavePath))
            File.Delete(SavePath);

        if (PlayerManager.Instance != null)
            PlayerManager.Instance.InitializePlayer();

        if (InventoryManager.Instance != null)
            InventoryManager.Instance.InitializeInventory();

        isDirty = false;

        Debug.Log("저장 파일 삭제 완료");
    }
}