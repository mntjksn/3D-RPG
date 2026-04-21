using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    public static PlayerManager Instance { get; private set; }

    private PlayerStat playerStat;
    private PlayerHealth playerHealth;

    public PlayerStat Stat => playerStat;
    public PlayerHealth Health => playerHealth;

    public float CurrentHp => playerStat != null ? playerStat.CurrentHp : 0f;
    public float MaxHp => playerStat != null ? playerStat.MaxHp : 0f;
    public float AttackPower => playerStat != null ? playerStat.AttackPower : 0f;
    public float ShieldPower => playerStat != null ? playerStat.ShieldPower : 0f;
    public float Speed => playerStat != null ? playerStat.Speed : 0f;
    public float Regen => playerStat != null ? playerStat.Regen : 0f;


    public int Level => playerStat != null ? playerStat.Level : 0;
    public int CurrentExp => playerStat != null ? playerStat.CurrentExp : 0;
    public int ExpToNextLevel => playerStat != null ? playerStat.GetExpToNextLevel() : 0;
    public int Gold => playerStat != null ? playerStat.Gold : 0;

    public bool IsDead => playerHealth != null && playerHealth.IsDead;

    private void Awake()
    {
        SetupSingleton();
        CacheComponents();
    }

    private void SetupSingleton()
    {
        if (Instance != null && Instance != this)
        {
            // gameObject 전체 삭제 대신 이 컴포넌트만 비활성화
            enabled = false;
            return;
        }
        Instance = this;
    }

    private void CacheComponents()
    {
        playerStat = GetComponent<PlayerStat>();
        playerHealth = GetComponent<PlayerHealth>();

        if (playerStat == null)
            Debug.LogWarning("PlayerStat 컴포넌트가 없습니다.");

        if (playerHealth == null)
            Debug.LogWarning("PlayerHealth 컴포넌트가 없습니다.");
    }

    public void InitializePlayer()
    {
        if (playerStat == null)
            return;

        playerStat.InitializeStat();
    }

    public void AddExp(int amount)
    {
        if (playerStat == null)
            return;

        playerStat.AddExp(amount);
        Debug.Log($"경험치 획득! 현재 EXP: {playerStat.CurrentExp}");

        if (SaveManager.Instance != null)
            SaveManager.Instance.MarkDirty();
    }

    public void AddGold(int amount)
    {
        if (playerStat == null)
            return;

        playerStat.AddGold(amount);
        Debug.Log($"골드 획득! 현재 Gold: {playerStat.Gold}");

        if (SaveManager.Instance != null)
            SaveManager.Instance.MarkDirty();
    }

    public PlayerSaveData GetSaveData()
    {
        if (playerStat == null)
            return null;

        return playerStat.GetSaveData();
    }

    public void LoadFromSaveData(PlayerSaveData saveData)
    {
        if (playerStat == null || saveData == null)
            return;

        playerStat.LoadFromSaveData(saveData);
    }
}