using System;
using UnityEngine;

[Serializable]
public class UpgradeLevelData
{
    [Header("Cost")]
    public int goldCost;

    [Range(0f, 1f)]
    public float successChance = 1f;

    [Header("Material")]
    public ItemData item;
    public int amount;
}