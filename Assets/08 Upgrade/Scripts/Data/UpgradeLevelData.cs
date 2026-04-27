using System;
using UnityEngine;

// 업그레이드 단계별 비용/확률/재료 데이터
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