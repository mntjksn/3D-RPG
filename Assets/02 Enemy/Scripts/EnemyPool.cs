using UnityEngine;

// 적 오브젝트 풀 참조 컴포넌트 - 스포너에 자신을 반환하는 역할
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

    public EnemyData GetEnemyData() => enemyData;
    public Vector3 GetLastSpawnPosition() => lastSpawnPosition;

    public void SetLastSpawnPosition(Vector3 pos)
    {
        lastSpawnPosition = pos;
    }

    // 사망 후 스포너의 풀로 반환
    public void ReturnToPool()
    {
        ownerSpawner?.ReturnEnemy(this);
    }
}