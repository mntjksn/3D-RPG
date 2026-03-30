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

    public void SavePlayer()
    {
        if (PlayerManager.Instance == null)
            return;

        PlayerSaveData saveData = PlayerManager.Instance.GetSaveData();
        if (saveData == null)
            return;

        string json = JsonUtility.ToJson(saveData, true);
        File.WriteAllText(SavePath, json);

        Debug.Log($"저장 완료: {SavePath}");
    }

    public void LoadPlayer()
    {
        if (PlayerManager.Instance == null)
            return;

        if (!File.Exists(SavePath))
        {
            Debug.Log("저장 파일이 없습니다.");
            return;
        }

        string json = File.ReadAllText(SavePath);
        PlayerSaveData saveData = JsonUtility.FromJson<PlayerSaveData>(json);

        PlayerManager.Instance.LoadFromSaveData(saveData);
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