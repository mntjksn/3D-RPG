using UnityEngine;

// 플레이어 관련 컴포넌트를 통합 관리하는 싱글톤 매니저
public class PlayerManager : MonoBehaviour
{
    public static PlayerManager Instance { get; private set; }

    private PlayerStat playerStat;
    private PlayerHealth playerHealth;

    // 외부 접근용 컴포넌트 참조
    public PlayerStat Stat => playerStat;
    public PlayerHealth Health => playerHealth;

    // PlayerStat 값을 외부에서 읽을 수 있도록 위임 프로퍼티
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

    // 싱글톤 설정 - 중복 인스턴스는 컴포넌트만 비활성화 (오브젝트는 유지)
    private void SetupSingleton()
    {
        if (Instance != null && Instance != this)
        {
            enabled = false;
            return;
        }
        Instance = this;
    }

    // 같은 오브젝트에서 필요한 컴포넌트 캐싱
    private void CacheComponents()
    {
        playerStat = GetComponent<PlayerStat>();
        playerHealth = GetComponent<PlayerHealth>();

        if (playerStat == null)
            Debug.LogWarning("PlayerStat 컴포넌트가 없습니다.");
        if (playerHealth == null)
            Debug.LogWarning("PlayerHealth 컴포넌트가 없습니다.");
    }

    // 플레이어 스탯 초기화 (게임 시작 또는 재시작 시 호출)
    public void InitializePlayer()
    {
        if (playerStat == null) return;
        playerStat.InitializeStat();
    }

    // 경험치 추가 후 세이브 데이터 갱신 예약
    public void AddExp(int amount)
    {
        if (playerStat == null) return;
        playerStat.AddExp(amount);
        SaveManager.Instance?.MarkDirty();
    }

    // 골드 추가 후 세이브 데이터 갱신 예약
    public void AddGold(int amount)
    {
        if (playerStat == null) return;
        playerStat.AddGold(amount);
        SaveManager.Instance?.MarkDirty();
    }

    // 현재 플레이어 상태를 세이브 데이터로 반환
    public PlayerSaveData GetSaveData()
    {
        return playerStat != null ? playerStat.GetSaveData() : null;
    }

    // 세이브 데이터를 플레이어 스탯에 적용
    public void LoadFromSaveData(PlayerSaveData saveData)
    {
        if (playerStat == null || saveData == null) return;
        playerStat.LoadFromSaveData(saveData);
    }
}