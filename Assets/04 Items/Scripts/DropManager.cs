using System.Collections.Generic;
using UnityEngine;

public class DropManager : MonoBehaviour
{
    public static DropManager Instance;

    [Header("Drop Prefabs")]
    [SerializeField] private WorldDrop goldDropPrefab;
    [SerializeField] private WorldDrop itemDropPrefab;

    [Header("Drop Spread")]
    [SerializeField] private float dropRadius = 0.8f;

    private void Awake()
    {
        Instance = this;
    }

    public void SpawnDrops(Vector3 position, int goldAmount, List<(ItemData itemData, int amount)> items)
    {
        if (goldAmount > 0)
        {
            SpawnGold(position, goldAmount);
        }

        if (items == null)
            return;

        foreach (var item in items)
        {
            SpawnItem(position, item.itemData, item.amount);
        }
    }

    private void SpawnGold(Vector3 center, int goldAmount)
    {
        if (goldDropPrefab == null)
            return;

        Vector3 spawnPos = center + GetRandomOffset();
        WorldDrop drop = Instantiate(goldDropPrefab, spawnPos, Quaternion.identity);
        drop.SetupGold(goldAmount);
    }

    private void SpawnItem(Vector3 center, ItemData itemData, int amount)
    {
        if (itemDropPrefab == null || itemData == null || amount <= 0)
            return;

        Vector3 spawnPos = center + GetRandomOffset();
        WorldDrop drop = Instantiate(itemDropPrefab, spawnPos, Quaternion.identity);
        drop.SetupItem(itemData, amount);
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