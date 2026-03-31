using System.Collections.Generic;
using UnityEngine;

public static class EnemyDropResolver
{
    public static int RollGold(EnemyData enemyData)
    {
        if (enemyData == null)
            return 0;

        return Random.Range(enemyData.minGold, enemyData.maxGold + 1);
    }

    public static List<(ItemData itemData, int amount)> RollDrops(EnemyData enemyData)
    {
        List<(ItemData itemData, int amount)> results = new();

        if (enemyData == null || enemyData.normalDrops == null)
            return results;

        foreach (var drop in enemyData.normalDrops)
        {
            if (drop == null || drop.itemData == null)
                continue;

            if (Random.value > drop.dropChance)
                continue;

            int amount = Random.Range(drop.minAmount, drop.maxAmount + 1);
            if (amount <= 0)
                continue;

            results.Add((drop.itemData, amount));
        }

        return results;
    }
}