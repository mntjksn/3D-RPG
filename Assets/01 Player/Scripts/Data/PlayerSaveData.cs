using System.Collections.Generic;

// 플레이어 전체 저장 데이터
[System.Serializable]
public class PlayerSaveData
{
    public int level;          // 현재 레벨
    public int currentExp;     // 현재 경험치
    public float currentHp;    // 현재 체력
    public int gold;           // 골드

    public List<InventoryItemSaveData> inventoryItems = new();   // 인벤토리
    public EquipmentSaveData equipmentData = new();              // 장비
    public PotionSlotSaveData potionSlot = new();                // 포션 슬롯
    public UpgradeSaveData upgradeData = new();                  // 업그레이드
    public QuestSaveData questData = new();                      // 퀘스트
}