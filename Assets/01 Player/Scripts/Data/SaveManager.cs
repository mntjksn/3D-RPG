using System.IO;
using UnityEngine;

public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance { get; private set; }

    private string SavePath => Path.Combine(Application.persistentDataPath, "player_save.json");

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

    private void Start()
    {
        LoadPlayer();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F5))
            SavePlayer();

        if (Input.GetKeyDown(KeyCode.F9))
            LoadPlayer();

        if (Input.GetKeyDown(KeyCode.F10))
            DeleteSave();
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

        string json = JsonUtility.ToJson(saveData, true);
        File.WriteAllText(SavePath, json);

        Debug.Log($"저장 완료: {SavePath}");
    }

    public void LoadPlayer()
    {
        if (!File.Exists(SavePath))
        {
            Debug.Log("저장 파일이 없습니다.");
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

        Debug.Log("불러오기 완료");
    }

    public void DeleteSave()
    {
        if (!File.Exists(SavePath))
            return;

        File.Delete(SavePath);
        Debug.Log("저장 파일 삭제 완료");
    }
}