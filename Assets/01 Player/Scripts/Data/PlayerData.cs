using UnityEngine;

[CreateAssetMenu(fileName = "PlayerData", menuName = "Game/Player Data")]
// 플레이어 기본 스탯 및 레벨 데이터
public class PlayerData : ScriptableObject
{
    [Header("Base Stats")]
    public int maxHp = 100;        // 최대 체력
    public int attackPower = 10;   // 공격력
    public int shieldPower = 20;   // 방어력
    public int speed = 8;          // 이동 속도
    public float regen = 0.01f;    // 체력 회복 비율 (초당)

    [Header("Level")]
    public int startLevel = 1;     // 시작 레벨
    public int expToLevelUp = 10;  // 레벨업 필요 경험치
}