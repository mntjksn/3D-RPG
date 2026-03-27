using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemySpawner : MonoBehaviour
{
    [Header("Spawn Enemy Data")]
    [SerializeField] private EnemyData[] enemyDatas;

    [Header("Spawn Settings")]
    [SerializeField] private int spawnCount = 5;
    [SerializeField] private Vector3 spawnArea = new Vector3(20f, 0f, 20f);
    [SerializeField] private float sampleDistance = 1f;
    [SerializeField] private float minSpawnDistance = 2.5f;
    [SerializeField] private int maxTryCount = 30;

    private readonly List<Vector3> spawnedPositions = new List<Vector3>();

    private void Start()
    {
        SpawnEnemies();
    }

    public void SpawnEnemies()
    {
        if (enemyDatas == null || enemyDatas.Length == 0)
        {
            Debug.LogWarning("EnemyData가 비어 있습니다.");
            return;
        }

        spawnedPositions.Clear();

        for (int i = 0; i < spawnCount; i++)
        {
            SpawnOneEnemy();
        }
    }

    private void SpawnOneEnemy()
    {
        EnemyData selectedData = enemyDatas[Random.Range(0, enemyDatas.Length)];

        if (selectedData == null || selectedData.prefab == null)
        {
            Debug.LogWarning("EnemyData 또는 prefab이 비어 있습니다.");
            return;
        }

        for (int i = 0; i < maxTryCount; i++)
        {
            Vector3 randomPoint = GetRandomPoint();

            if (!NavMesh.SamplePosition(randomPoint, out NavMeshHit hit, sampleDistance, NavMesh.AllAreas))
                continue;

            if (!IsFarEnough(hit.position))
                continue;

            Instantiate(selectedData.prefab, hit.position, Quaternion.identity);
            spawnedPositions.Add(hit.position);
            return;
        }

        Debug.LogWarning("적절한 스폰 위치를 찾지 못했습니다.");
    }

    private Vector3 GetRandomPoint()
    {
        float randomX = Random.Range(-spawnArea.x * 0.5f, spawnArea.x * 0.5f);
        float randomZ = Random.Range(-spawnArea.z * 0.5f, spawnArea.z * 0.5f);

        return transform.position + new Vector3(randomX, 0f, randomZ);
    }

    private bool IsFarEnough(Vector3 pos)
    {
        for (int i = 0; i < spawnedPositions.Count; i++)
        {
            if (Vector3.Distance(pos, spawnedPositions[i]) < minSpawnDistance)
                return false;
        }

        return true;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(transform.position, spawnArea);
    }
}