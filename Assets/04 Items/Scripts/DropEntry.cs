using System;
using UnityEngine;

[Serializable]
public class DropEntry
{
    public ItemData itemData;

    [Range(0f, 1f)]
    public float dropChance = 1f;

    public int minAmount = 1;
    public int maxAmount = 1;
}