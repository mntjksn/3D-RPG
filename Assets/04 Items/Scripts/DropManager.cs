using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class DropManager : MonoBehaviour
{
    public static DropManager Instance;

    [Header("Drop Prefab Names")]
    [SerializeField] private string goldDropPrefabName = "WorldDrop_Gold";
    [SerializeField] private string itemDropPrefabName = "WorldDrop_Item";

    [Header("Drop Spread")]
    [SerializeField] private float dropRadius = 0.8f;

    private void Awake()
    {
        Instance = this;
    }

    public void SpawnDrops(Vector3 position, int goldAmount, List<(ItemData itemData, int amount)> items, int ownerActorNumber)
    {
        if (!PhotonNetwork.IsMasterClient)
            return;

        if (goldAmount > 0)
            SpawnGold(position, goldAmount, ownerActorNumber);

        if (items == null)
            return;

        foreach (var item in items)
            SpawnItem(position, item.itemData, item.amount, ownerActorNumber);
    }

    private void SpawnGold(Vector3 center, int goldAmount, int ownerActorNumber)
    {
        Vector3 spawnPos = center + Vector3.up * 0.5f + GetRandomOffset();

        GameObject obj = PhotonNetwork.InstantiateRoomObject(
            goldDropPrefabName,
            spawnPos,
            Quaternion.identity
        );

        WorldDrop drop = obj.GetComponent<WorldDrop>();
        if (drop != null)
            drop.SetupGold(goldAmount, ownerActorNumber);
    }

    private void SpawnItem(Vector3 center, ItemData itemData, int amount, int ownerActorNumber)
    {
        if (itemData == null || amount <= 0)
            return;

        Vector3 spawnPos = center + Vector3.up * 0.5f + GetRandomOffset();

        GameObject obj = PhotonNetwork.InstantiateRoomObject(
            itemDropPrefabName,
            spawnPos,
            Quaternion.identity
        );

        WorldDrop drop = obj.GetComponent<WorldDrop>();
        if (drop != null)
            drop.SetupItem(itemData, amount, ownerActorNumber);
    }

    private Vector3 GetRandomOffset()
    {
        return new Vector3(
            Random.Range(-dropRadius, dropRadius),
            0f,
            Random.Range(-dropRadius, dropRadius)
        );
    }
}