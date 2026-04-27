using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Photon.Pun;

// 적 생성, 풀 관리, 네트워크 동기화 담당
public class EnemySpawner : MonoBehaviour
{
    [Header("Spawner Settings")]
    [SerializeField] private byte spawnerIndex;

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

    private readonly List<Vector3> spawnedPositions = new List<Vector3>();
    private readonly Dictionary<EnemyData, Queue<GameObject>> poolDictionary = new Dictionary<EnemyData, Queue<GameObject>>();
    private readonly Dictionary<EnemyData, List<GameObject>> poolListDictionary = new Dictionary<EnemyData, List<GameObject>>();

    private EnemySpawnerNetworkSync networkSync;

    public byte SpawnerIndex => spawnerIndex;
    public EnemyData[] EnemyDatas => enemyDatas;

    private void Awake()
    {
        networkSync = GetComponent<EnemySpawnerNetworkSync>();
    }

    private void Start()
    {
        CreatePools();
    }

    // 풀 생성
    private void CreatePools()
    {
        if (!HasEnemyData())
            return;

        foreach (EnemyData data in enemyDatas)
        {
            if (!IsValidEnemyData(data))
                continue;

            poolListDictionary[data] = new List<GameObject>();
            poolDictionary[data] = CreatePool(data);
        }
    }

    // 타입별 풀 생성
    private Queue<GameObject> CreatePool(EnemyData data)
    {
        Queue<GameObject> pool = new Queue<GameObject>();

        for (int i = 0; i < poolSizePerType; i++)
        {
            GameObject obj = Instantiate(data.prefab, enemyParent);
            obj.SetActive(false);

            PhotonView pv = obj.GetComponent<PhotonView>();
            if (pv != null && PhotonNetwork.IsMasterClient)
                PhotonNetwork.AllocateViewID(pv);

            EnemyPool member = obj.GetComponent<EnemyPool>();
            if (member == null)
                member = obj.AddComponent<EnemyPool>();

            member.Initialize(this, data);

            pool.Enqueue(obj);
            poolListDictionary[data].Add(obj);
        }

        return pool;
    }

    // 초기 스폰 후 전체 클라에 브로드캐스트
    public void SpawnInitialEnemiesAndBroadcast()
    {
        if (!HasEnemyData())
            return;

        spawnedPositions.Clear();

        for (int i = 0; i < spawnCount; i++)
        {
            int dataIndex;
            int poolIndex;
            Vector3 spawnPos;
            int viewId;

            if (TrySpawnEnemy(out dataIndex, out poolIndex, out spawnPos, out viewId))
            {
                if (networkSync != null)
                    networkSync.BroadcastActivate(dataIndex, poolIndex, spawnPos, viewId);
            }
        }
    }

    // 랜덤 타입 스폰 시도
    public bool TrySpawnEnemy(out int dataIndex, out int poolIndex, out Vector3 spawnPos, out int viewId)
    {
        dataIndex = -1;
        poolIndex = -1;
        spawnPos = Vector3.zero;
        viewId = -1;

        if (!HasEnemyData())
            return false;

        EnemyData selectedData = enemyDatas[Random.Range(0, enemyDatas.Length)];
        return TrySpawnEnemy(selectedData, out dataIndex, out poolIndex, out spawnPos, out viewId);
    }

    // 특정 타입 스폰 시도
    public bool TrySpawnEnemy(EnemyData selectedData, out int dataIndex, out int poolIndex, out Vector3 spawnPos, out int viewId)
    {
        dataIndex = -1;
        poolIndex = -1;
        spawnPos = Vector3.zero;
        viewId = -1;

        if (!IsValidEnemyData(selectedData))
            return false;

        if (!TryGetSpawnPosition(out spawnPos))
            return false;

        GameObject enemyObj = GetFromPool(selectedData);
        if (enemyObj == null)
            return false;

        dataIndex = GetDataIndex(selectedData);
        poolIndex = GetPoolIndex(selectedData, enemyObj);

        PhotonView pv = enemyObj.GetComponent<PhotonView>();
        viewId = pv != null ? pv.ViewID : -1;

        ActivateEnemy(dataIndex, poolIndex, spawnPos, viewId);
        spawnedPositions.Add(spawnPos);

        return true;
    }

    // 사망 후 리스폰 요청
    public void RequestRespawn(EnemyData deadEnemyData)
    {
        if (!PhotonNetwork.IsMasterClient)
            return;

        if (deadEnemyData == null)
            return;

        StartCoroutine(RespawnRoutine(deadEnemyData));
    }

    private IEnumerator RespawnRoutine(EnemyData deadEnemyData)
    {
        yield return new WaitForSeconds(deadEnemyData.respawnDelay);

        int dataIndex;
        int poolIndex;
        Vector3 spawnPos;
        int viewId;

        if (TrySpawnEnemy(deadEnemyData, out dataIndex, out poolIndex, out spawnPos, out viewId))
        {
            if (networkSync != null)
                networkSync.BroadcastActivate(dataIndex, poolIndex, spawnPos, viewId);
        }
    }

    public void ReturnEnemy(EnemyPool member)
    {
        if (member == null)
            return;

        if (!PhotonNetwork.IsMasterClient)
            return;

        EnemyData data = member.GetEnemyData();
        int dataIndex = GetDataIndex(data);
        int poolIndex = GetPoolIndex(data, member.gameObject);

        ReturnEnemyByIndex(dataIndex, poolIndex);

        if (networkSync != null)
            networkSync.BroadcastReturn(dataIndex, poolIndex);
    }

    // 적 활성화
    public void ActivateEnemy(int dataIndex, int poolIndex, Vector3 spawnPos, int viewId)
    {
        if (enemyDatas == null || dataIndex < 0 || dataIndex >= enemyDatas.Length)
            return;

        EnemyData data = enemyDatas[dataIndex];

        List<GameObject> list;
        if (!poolListDictionary.TryGetValue(data, out list))
            return;

        if (poolIndex < 0 || poolIndex >= list.Count)
            return;

        GameObject enemyObj = list[poolIndex];
        enemyObj.transform.position = spawnPos;
        enemyObj.transform.rotation = Quaternion.identity;
        enemyObj.SetActive(true);

        CapsuleCollider col = enemyObj.GetComponent<CapsuleCollider>();
        if (col != null)
            col.enabled = true;

        EnemyPool poolMember = enemyObj.GetComponent<EnemyPool>();
        if (poolMember != null)
            poolMember.SetLastSpawnPosition(spawnPos);

        EnemyHealth enemyHealth = enemyObj.GetComponent<EnemyHealth>();
        if (enemyHealth != null)
        {
            enemyHealth.SetData(data, spawnerIndex, poolIndex);
            enemyHealth.SetSpawner(this);
        }

        EnemyAI enemyAI = enemyObj.GetComponent<EnemyAI>();
        if (enemyAI != null)
            enemyAI.SetData(data, spawnerIndex, poolIndex);
    }

    // 인덱스로 반환 처리
    public void ReturnEnemyByIndex(int dataIndex, int poolIndex)
    {
        if (enemyDatas == null || dataIndex < 0 || dataIndex >= enemyDatas.Length)
            return;

        EnemyData data = enemyDatas[dataIndex];

        List<GameObject> list;
        if (!poolListDictionary.TryGetValue(data, out list))
            return;

        if (poolIndex < 0 || poolIndex >= list.Count)
            return;

        GameObject enemyObj = list[poolIndex];

        EnemyPool member = enemyObj.GetComponent<EnemyPool>();
        RemoveSpawnPosition(member);
        StopAgent(enemyObj);
        enemyObj.SetActive(false);
    }

    // 새 플레이어에게 현재 상태 전달
    public void SendCurrentStateToPlayer(Photon.Realtime.Player newPlayer)
    {
        if (networkSync == null)
            return;

        foreach (KeyValuePair<EnemyData, List<GameObject>> kvp in poolListDictionary)
        {
            EnemyData data = kvp.Key;
            List<GameObject> list = kvp.Value;
            int dataIndex = GetDataIndex(data);

            for (int i = 0; i < list.Count; i++)
            {
                if (!list[i].activeSelf)
                    continue;

                PhotonView pv = list[i].GetComponent<PhotonView>();
                int viewId = pv != null ? pv.ViewID : -1;
                Vector3 pos = list[i].transform.position;

                networkSync.SendActivateToPlayer(newPlayer, dataIndex, i, pos, viewId);
            }
        }
    }

    // 스폰 위치 탐색
    private bool TryGetSpawnPosition(out Vector3 spawnPosition)
    {
        for (int i = 0; i < maxTryCount; i++)
        {
            Vector3 randomPoint = GetRandomPoint();

            NavMeshHit hit;
            if (!NavMesh.SamplePosition(randomPoint, out hit, sampleDistance, NavMesh.AllAreas))
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

    // 풀에서 비활성 오브젝트 찾기
    private GameObject GetFromPool(EnemyData data)
    {
        Queue<GameObject> pool;
        if (!poolDictionary.TryGetValue(data, out pool))
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

    public int GetDataIndex(EnemyData data)
    {
        if (enemyDatas == null)
            return -1;

        for (int i = 0; i < enemyDatas.Length; i++)
        {
            if (enemyDatas[i] == data)
                return i;
        }

        return -1;
    }

    public int GetPoolIndex(EnemyData data, GameObject obj)
    {
        List<GameObject> list;
        if (!poolListDictionary.TryGetValue(data, out list))
            return -1;

        return list.IndexOf(obj);
    }

    private void RemoveSpawnPosition(EnemyPool member)
    {
        if (member == null)
            return;

        Vector3 lastSpawnPos = member.GetLastSpawnPosition();
        spawnedPositions.Remove(lastSpawnPos);
    }

    private void StopAgent(GameObject enemyObj)
    {
        NavMeshAgent agent = enemyObj.GetComponent<NavMeshAgent>();
        if (agent != null && agent.enabled && agent.isOnNavMesh)
        {
            agent.ResetPath();
            agent.isStopped = true;
        }
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