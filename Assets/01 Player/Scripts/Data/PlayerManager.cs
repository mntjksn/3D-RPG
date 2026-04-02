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
            Debug.LogWarning("PlayerStat ÄÄÆ÷³ÍÆ®°¡ ¾ø½À´Ï´Ù.");

        if (playerHealth == null)
            Debug.LogWarning("PlayerHealth ÄÄÆ÷³ÍÆ®°¡ ¾ø½À´Ï´Ù.");
    }

    public void AddExp(int amount)
    {
        if (playerStat == null)
            return;

        playerStat.AddExp(amount);
        Debug.Log($"°æÇèÄ¡ È¹µæ! ÇöÀç EXP: {playerStat.CurrentExp}");
    }

    public void AddGold(int amount)
    {
        if (playerStat == null)
            return;

        playerStat.AddGold(amount);
        Debug.Log($"°ñµå È¹µæ! ÇöÀç Gold: {playerStat.Gold}");
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