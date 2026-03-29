using System.Collections;
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
    [SerializeField] private Transform enemyParent;

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

    public void RequestRespawn(EnemyData deadEnemyData)
    {
        if (deadEnemyData == null)
            return;

        StartCoroutine(RespawnRoutine(deadEnemyData));
    }

    private IEnumerator RespawnRoutine(EnemyData deadEnemyData)
    {
        yield return new WaitForSeconds(deadEnemyData.spawnTime);
        SpawnOneEnemy(deadEnemyData);
    }

    private void SpawnOneEnemy()
    {
        EnemyData selectedData = enemyDatas[Random.Range(0, enemyDatas.Length)];
        SpawnOneEnemy(selectedData);
    }

    private void SpawnOneEnemy(EnemyData selectedData)
    {
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

            if (!IsInsideSpawnArea(hit.position))
                continue;

            if (!IsFarEnough(hit.position))
                continue;

            GameObject enemyObj = Instantiate(selectedData.prefab, hit.position, Quaternion.identity, enemyParent);

            EnemyAI enemyAI = enemyObj.GetComponent<EnemyAI>();
            if (enemyAI != null)
                enemyAI.SetData(selectedData);

            EnemyHealth enemyHealth = enemyObj.GetComponent<EnemyHealth>();
            if (enemyHealth != null)
            {
                enemyHealth.SetData(selectedData);
                enemyHealth.SetSpawner(this);
            }

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

    private bool IsInsideSpawnArea(Vector3 pos)
    {
        Vector3 localPos = pos - transform.position;

        return Mathf.Abs(localPos.x) <= spawnArea.x * 0.5f &&
               Mathf.Abs(localPos.z) <= spawnArea.z * 0.5f;
    }

    private bool IsFarEnough(Vector3 pos)
    {
        Vector2 pos2D = new Vector2(pos.x, pos.z);

        for (int i = 0; i < spawnedPositions.Count; i++)
        {
            Vector2 spawned2D = new Vector2(spawnedPositions[i].x, spawnedPositions[i].z);

            if (Vector2.Distance(pos2D, spawned2D) < minSpawnDistance)
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