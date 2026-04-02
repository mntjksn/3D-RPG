using UnityEngine;

[CreateAssetMenu(fileName = "PlayerData", menuName = "Game/Player Data")]
public class PlayerData : ScriptableObject
{
    [Header("Base Stats")]
    public float maxHp = 100f;
    public float attackPower = 10f;
    public float shieldPower = 20f;
    public float speed = 8f;

    [Header("Level")]
    public int startLevel = 1;
    public int expToLevelUp = 10;
}
