using UnityEngine;

// 아이템 종류
public enum ItemType
{
    Material = 0,
    Consumable,
    Weapon,
    Armor,
    Shoes,
    Shield
}

// 장착 슬롯 종류
public enum EquipmentSlotType
{
    None = 0,
    Weapon,
    Armor,
    Shoes,
    Shield
}

[CreateAssetMenu(fileName = "NewItem", menuName = "Game/Item Data")]
// 아이템 기본 데이터
public class ItemData : ScriptableObject
{
    [Header("Common")]
    public string itemId;              // 고유 ID
    public string itemName;            // 이름
    [TextArea] public string description;
    public Sprite icon;
    public ItemType itemType;
    public int buyPrice = 1;
    public int sellPrice = 1;

    [Header("Inventory")]
    public bool isStackable = true;    // 중첩 가능 여부
    public int maxStack = 99;

    [Header("Equip")]
    public EquipmentSlotType equipSlot = EquipmentSlotType.None;

    [Header("Equip Stats")]
    public int attackPower;
    public int shieldPower;
    public int moveSpeedBonus;
    public int maxHpBonus;
}