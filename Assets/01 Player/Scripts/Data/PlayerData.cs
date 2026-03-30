using UnityEngine;

[CreateAssetMenu(fileName = "PlayerData", menuName = "Game/Player Data")]
public class PlayerData : ScriptableObject
{
    [Header("Base Stats")]
    public float maxHp = 100f;
    public float attackPower = 10f;

    [Header("Level")]
    public int startLevel = 1;
    public int expToLevelUp = 100;

    [Header("Growth")]
    public float hpPerLevel = 10f;
    public float attackPerLevel = 2f;
}
