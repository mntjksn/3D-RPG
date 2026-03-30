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

    public int Level => playerStat != null ? playerStat.Level : 0;
    public int CurrentExp => playerStat != null ? playerStat.CurrentExp : 0;
    public int ExpToNextLevel => playerStat != null ? playerStat.GetExpToNextLevel() : 0;

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
            Destroy(gameObject);
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

    public void AddExp(int amount)
    {
        if (playerStat == null)
            return;

        playerStat.AddExp(amount);
        Debug.Log($"경험치 획득! 현재 EXP: {playerStat.CurrentExp}");
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