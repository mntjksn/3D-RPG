using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ShopData", menuName = "Game/Shop Data")]
// 상점에 판매되는 아이템 목록
public class ShopData : ScriptableObject
{
    public List<ShopItemEntry> items = new();
}

[Serializable]
// 상점 아이템 정보
public class ShopItemEntry
{
    public ItemData itemData;
}