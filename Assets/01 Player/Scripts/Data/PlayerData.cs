using UnityEngine;

[CreateAssetMenu(fileName = "PlayerData", menuName = "Game/Player Data")]
public class PlayerData : ScriptableObject
{
    [Header("Base Stats")]
    public int maxHp = 100;
    public int attackPower = 10;
    public int shieldPower = 20;
    public int speed = 8;
    public float regen = 0.01f;

    [Header("Level")]
    public int startLevel = 1;
    public int expToLevelUp = 10;
}
