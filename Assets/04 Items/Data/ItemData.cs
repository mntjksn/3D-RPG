using UnityEngine;

public enum ItemType
{
    Material,
    Weapon,
    Armor,
    Shoes,
    Shield
}

public enum EquipmentSlotType
{
    None,
    Weapon,
    Armor,
    Shoes,
    Shield
}

[CreateAssetMenu(fileName = "NewItem", menuName = "Game/Item Data")]
public class ItemData : ScriptableObject
{
    [Header("Common")]
    public string itemId;
    public string itemName;
    [TextArea] public string description;
    public Sprite icon;
    public ItemType itemType;
    public int sellPrice = 1;

    [Header("Inventory")]
    public bool isStackable = true;
    public int maxStack = 99;

    [Header("Equip")]
    public EquipmentSlotType equipSlot = EquipmentSlotType.None;

    [Header("Equip Stats")]
    public int attackPower;
    public int defensePower;
    public float moveSpeedBonus;
    public int maxHpBonus;
}