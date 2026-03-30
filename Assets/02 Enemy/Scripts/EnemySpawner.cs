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

    [Header("Pool Settings")]
    [SerializeField] private int poolSizePerType = 10;

    private readonly List<Vector3> spawnedPositions = new();
    private readonly Dictionary<EnemyData, Queue<GameObject>> poolDictionary = new();

    private void Start()
    {
        CreatePools();
        SpawnEnemies();
    }

    private void CreatePools()
    {
        if (!HasEnemyData())
            return;

        foreach (EnemyData data in enemyDatas)
        {
            if (!IsValidEnemyData(data))
                continue;

            poolDictionary[data] = CreatePool(data);
        }
    }

    private Queue<GameObject> CreatePool(EnemyData data)
    {
        Queue<GameObject> pool = new();

        for (int i = 0; i < poolSizePerType; i++)
        {
            GameObject obj = Instantiate(data.prefab, enemyParent);
            obj.SetActive(false);

            EnemyPool member = obj.GetComponent<EnemyPool>();
            if (member == null)
                member = obj.AddComponent<EnemyPool>();

            member.Initialize(this, data);
            pool.Enqueue(obj);
        }

        return pool;
    }

    public void SpawnEnemies()
    {
        if (!HasEnemyData())
        {
            Debug.LogWarning("EnemyData가 비어 있습니다.");
            return;
        }

        spawnedPositions.Clear();

        for (int i = 0; i < spawnCount; i++)
            SpawnOneEnemy();
    }

    public void RequestRespawn(EnemyData deadEnemyData)
    {
        if (deadEnemyData == null)
            return;

        StartCoroutine(RespawnRoutine(deadEnemyData));
    }

    private IEnumerator RespawnRoutine(EnemyData deadEnemyData)
    {
        yield return new WaitForSeconds(deadEnemyData.respawnDelay);
        SpawnOneEnemy(deadEnemyData);
    }

    private void SpawnOneEnemy()
    {
        EnemyData selectedData = enemyDatas[Random.Range(0, enemyDatas.Length)];
        SpawnOneEnemy(selectedData);
    }

    private void SpawnOneEnemy(EnemyData selectedData)
    {
        if (!IsValidEnemyData(selectedData))
        {
            Debug.LogWarning("EnemyData 또는 prefab이 비어 있습니다.");
            return;
        }

        if (!TryGetSpawnPosition(out Vector3 spawnPosition))
        {
            Debug.LogWarning("적절한 스폰 위치를 찾지 못했습니다.");
            return;
        }

        GameObject enemyObj = GetFromPool(selectedData);
        if (enemyObj == null)
        {
            Debug.LogWarning($"{selectedData.name} 풀에 사용할 오브젝트가 없습니다.");
            return;
        }

        ActivateEnemy(enemyObj, selectedData, spawnPosition);
        spawnedPositions.Add(spawnPosition);
    }

    // 스폰 위치 찾기
    private bool TryGetSpawnPosition(out Vector3 spawnPosition)
    {
        for (int i = 0; i < maxTryCount; i++)
        {
            Vector3 randomPoint = GetRandomPoint();

            if (!NavMesh.SamplePosition(randomPoint, out NavMeshHit hit, sampleDistance, NavMesh.AllAreas))
                continue;

            if (!IsInsideSpawnArea(hit.position))
                continue;

            if (!IsFarEnough(hit.position))
                continue;

            spawnPosition = hit.position;
            return true;
        }

        spawnPosition = Vector3.zero;
        return false;
    }

    // 오브젝트 활성화 및 초기화
    private void ActivateEnemy(GameObject enemyObj, EnemyData data, Vector3 spawnPosition)
    {
        enemyObj.transform.position = spawnPosition;
        enemyObj.transform.rotation = Quaternion.identity;
        enemyObj.SetActive(true);

        EnemyPool poolMember = enemyObj.GetComponent<EnemyPool>();
        poolMember?.SetLastSpawnPosition(spawnPosition);

        EnemyHealth enemyHealth = enemyObj.GetComponent<EnemyHealth>();
        if (enemyHealth != null)
        {
            enemyHealth.SetData(data);
            enemyHealth.SetSpawner(this);
        }

        EnemyAI enemyAI = enemyObj.GetComponent<EnemyAI>();
        enemyAI?.SetData(data);
    }

    private GameObject GetFromPool(EnemyData data)
    {
        if (!poolDictionary.TryGetValue(data, out Queue<GameObject> pool))
            return null;

        int count = pool.Count;

        for (int i = 0; i < count; i++)
        {
            GameObject obj = pool.Dequeue();
            pool.Enqueue(obj);

            if (!obj.activeSelf)
                return obj;
        }

        return null;
    }

    public void ReturnEnemy(EnemyPool member)
    {
        if (member == null)
            return;

        GameObject enemyObj = member.gameObject;
        RemoveSpawnPosition(member);
        StopAgent(enemyObj);
        enemyObj.SetActive(false);

        EnemyData data = member.GetEnemyData();
        EnsurePoolExists(data);
    }

    // 스폰 위치 제거
    private void RemoveSpawnPosition(EnemyPool member)
    {
        Vector3 lastSpawnPos = member.GetLastSpawnPosition();
        spawnedPositions.Remove(lastSpawnPos);
    }

    // 에이전트 정지
    private void StopAgent(GameObject enemyObj)
    {
        NavMeshAgent agent = enemyObj.GetComponent<NavMeshAgent>();

        if (agent != null && agent.enabled && agent.isOnNavMesh)
        {
            agent.ResetPath();
            agent.isStopped = true;
        }
    }

    private void EnsurePoolExists(EnemyData data)
    {
        if (data != null && !poolDictionary.ContainsKey(data))
            poolDictionary[data] = new Queue<GameObject>();
    }

    private bool HasEnemyData()
    {
        return enemyDatas != null && enemyDatas.Length > 0;
    }

    private bool IsValidEnemyData(EnemyData data)
    {
        return data != null && data.prefab != null;
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
        Vector2 pos2D = new(pos.x, pos.z);

        for (int i = 0; i < spawnedPositions.Count; i++)
        {
            Vector2 spawned2D = new(spawnedPositions[i].x, spawnedPositions[i].z);

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