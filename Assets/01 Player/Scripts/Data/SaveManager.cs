using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance { get; private set; }

    private string SavePath => Path.Combine(Application.persistentDataPath, "player_save.json");

    private bool hasLoadedOnce = false;

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
        StartCoroutine(LoadPlayerRoutine());
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F5))
            SavePlayer();

        if (Input.GetKeyDown(KeyCode.F9))
            StartCoroutine(LoadPlayerRoutine());

        if (Input.GetKeyDown(KeyCode.F10))
            DeleteSave();
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (!hasLoadedOnce)
            StartCoroutine(LoadPlayerRoutine());
    }

    private IEnumerator LoadPlayerRoutine()
    {
        yield return null;

        float timeout = 3f;
        float timer = 0f;

        while ((PlayerManager.Instance == null || InventoryManager.Instance == null) && timer < timeout)
        {
            timer += Time.unscaledDeltaTime;
            yield return null;
        }

        LoadPlayer();
        hasLoadedOnce = true;
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
        else
        {
            Debug.LogWarning("PlayerManager.Instance가 없습니다. 플레이어 데이터 저장 실패");
        }

        if (InventoryManager.Instance != null)
        {
            saveData.inventoryItems = InventoryManager.Instance.GetSaveData();
        }
        else
        {
            Debug.LogWarning("InventoryManager.Instance가 없습니다. 인벤토리 데이터 저장 실패");
        }

        string json = JsonUtility.ToJson(saveData, true);
        File.WriteAllText(SavePath, json);

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
        else
            Debug.LogWarning("PlayerManager.Instance가 없어서 플레이어 데이터 로드 실패");

        if (InventoryManager.Instance != null)
            InventoryManager.Instance.LoadFromSaveData(saveData.inventoryItems);
        else
            Debug.LogWarning("InventoryManager.Instance가 없어서 인벤토리 데이터 로드 실패");

        Debug.Log("불러오기 완료");
    }

    public void DeleteSave()
    {
        if (File.Exists(SavePath))
        {
            File.Delete(SavePath);
            Debug.Log("저장 파일 삭제 완료");
        }

        if (PlayerManager.Instance != null)
            PlayerManager.Instance.InitializePlayer();

        if (InventoryManager.Instance != null)
            InventoryManager.Instance.InitializeInventory();
    }
}