using System;

// 장착 아이템 저장 데이터
[Serializable]
public class EquipmentSaveData
{
    public string weaponItemId;   // 무기
    public string armorItemId;    // 갑옷
    public string shoesItemId;    // 신발
    public string shieldItemId;   // 방패
}