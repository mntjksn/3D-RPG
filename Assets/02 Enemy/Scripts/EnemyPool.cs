using UnityEngine;

public class EnemyPool : MonoBehaviour
{
    private EnemySpawner ownerSpawner;
    private EnemyData enemyData;
    private Vector3 lastSpawnPosition;

    public void Initialize(EnemySpawner spawner, EnemyData data)
    {
        ownerSpawner = spawner;
        enemyData = data;
    }

    public EnemyData GetEnemyData()
    {
        return enemyData;
    }

    public void SetLastSpawnPosition(Vector3 pos)
    {
        lastSpawnPosition = pos;
    }

    public Vector3 GetLastSpawnPosition()
    {
        return lastSpawnPosition;
    }

    public void ReturnToPool()
    {
        ownerSpawner?.ReturnEnemy(this);
    }
}