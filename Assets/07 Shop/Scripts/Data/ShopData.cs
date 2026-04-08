using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ShopData", menuName = "Game/Shop Data")]
public class ShopData : ScriptableObject
{
    public List<ShopItemEntry> items = new List<ShopItemEntry>();
}

[Serializable]
public class ShopItemEntry
{
    public ItemData itemData;
}