using System.Collections.Generic;
using UnityEngine;
using static UnityEditor.Search.SearchColumn;

[CreateAssetMenu(fileName = "EnemyData", menuName = "Game/Enemy Data")]
public class EnemyData : ScriptableObject
{
    [Header("Info")]
    public string enemyName;
    public float maxHp = 100f;
    public int exp = 1;

    [Header("Gold Drop")]
    public int minGold = 1;
    public int maxGold = 5;

    [Header("Respawn")]
    public float deadBodyDuration = 2f;
    public float respawnDelay = 5f;

    [Header("Speed")]
    public float attackSpeed = 3f;
    public float moveSpeed = 4f;
    public float patrolSpeed = 2f;

    [Header("Range")]
    public float detectRange = 7f;
    public float loseRange = 10f;
    public float attackRange = 2f;

    [Header("Attack")]
    public float attackDamage = 10f;
    public float attackRecoverTime = 0.5f;
    public float attackRate = 1f;

    [Header("Patrol")]
    public float patrolRadius = 5f;
    public float patrolWaitTime = 2f;

    [Header("Drops")]
    public List<DropEntry> normalDrops = new List<DropEntry>();

    [Header("Prefab")]
    public GameObject prefab;
}