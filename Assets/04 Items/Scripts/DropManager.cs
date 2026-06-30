using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

// 골드와 아이템 드랍 생성 담당
public class DropManager : MonoBehaviour
{
    public static DropManager Instance { get; private set; }

    [Header("Drop Prefab Names")]
    [SerializeField] private string goldDropPrefabName = "WorldDrop_Gold";
    [SerializeField] private string itemDropPrefabName = "WorldDrop_Item";

    [Header("Drop Spread")]
    [SerializeField] private float dropRadius = 0.8f;

    private void Awake()
    {
        // 싱글톤 설정
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    // 드랍 목록 생성
    public void SpawnDrops(Vector3 position, int goldAmount, List<(ItemData itemData, int amount)> items, int ownerActorNumber)
    {
        if (!PhotonNetwork.IsMasterClient) return;

        if (goldAmount > 0)
            SpawnGold(position, goldAmount, ownerActorNumber);

        if (items == null) return;

        foreach (var item in items)
            SpawnItem(position, item.itemData, item.amount, ownerActorNumber);
    }

    // 골드 드랍 생성
    private void SpawnGold(Vector3 center, int goldAmount, int ownerActorNumber)
    {
        Vector3 spawnPos = center + Vector3.up * 0.5f + GetRandomOffset();

        GameObject obj = PhotonNetwork.InstantiateRoomObject(
            goldDropPrefabName,
            spawnPos,
            Quaternion.identity
        );

        obj.GetComponent<WorldDrop>()?.SetupGold(goldAmount, ownerActorNumber);
    }

    // 아이템 드랍 생성
    private void SpawnItem(Vector3 center, ItemData itemData, int amount, int ownerActorNumber)
    {
        if (itemData == null || amount <= 0) return;

        Vector3 spawnPos = center + Vector3.up * 0.5f + GetRandomOffset();

        GameObject obj = PhotonNetwork.InstantiateRoomObject(
            itemDropPrefabName,
            spawnPos,
            Quaternion.identity
        );

        obj.GetComponent<WorldDrop>()?.SetupItem(itemData, amount, ownerActorNumber);
    }

    // 랜덤 드랍 위치 계산
    private Vector3 GetRandomOffset()
    {
        return new Vector3(
            Random.Range(-dropRadius, dropRadius),
            0f,
            Random.Range(-dropRadius, dropRadius)
        );
    }
}