using System;
using UnityEngine;

// 아이템 드랍 정보
[Serializable]
public class DropEntry
{
    public ItemData itemData;

    [Range(0f, 1f)]
    public float dropChance = 1f;   // 드랍 확률 (0~1)

    public int minAmount = 1;       // 최소 수량
    public int maxAmount = 1;       // 최대 수량
}