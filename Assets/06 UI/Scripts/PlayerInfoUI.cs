using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerInfoUI : MonoBehaviour
{
    [Header("PlayerInfo UI")]
    [SerializeField] private GameObject playerInfoUI;

    [Header("Buttons")]
    [SerializeField] private Button closeButton;

    [Header("UI")]
    [SerializeField] private Transform basicParent;
    [SerializeField] private Transform detailParent;
    [SerializeField] private StatBlockUI statBlockPrefab;

    [Header("Player")]
    [SerializeField] private PlayerStat playerStat;
    [SerializeField] private PlayerActionLock playerActionLock;

    private readonly List<StatBlockUI> spawnedBlocks = new List<StatBlockUI>();

    private const int BaseAttack = 10;
    private const int BaseShield = 20;
    private const int BaseMaxHp = 100;
    private const double BaseRegen = 0.01;
    private const int BaseSpeed = 8;

    private void Start()
    {
        BindButtons();
    }

    private void OnEnable()
    {
        Refresh();

        Cursor.lockState = CursorLockMode.Confined;
        Cursor.visible = true;

        if (playerActionLock != null)
            playerActionLock.LockRecoverControls();
    }

    private void OnDisable()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (playerActionLock != null)
            playerActionLock.UnlockRecoverControls();
    }

    private void BindButtons()
    {
        if (closeButton != null)
            closeButton.onClick.AddListener(ClosePlayerInfo);
    }

    public void Refresh()
    {
        if (playerStat == null)
        {
            Debug.LogError("PlayerStat이 비어있음");
            return;
        }

        Clear();

        int currentAttack = playerStat.GetAttackPower();
        int currentShield = playerStat.GetShieldPower();
        int currentMaxHp = playerStat.GetMaxHp();
        double currentRegen = playerStat.GetRegen();
        int currentSpeed = playerStat.GetSpeed();

        int weaponAttackBonus = GetWeaponAttackBonus();
        int shieldDefenseBonus = GetShieldDefenseBonus();
        int armorHpBonus = GetArmorHpBonus();
        int shoesSpeedBonus = GetShoesSpeedBonus();

        int upgradeAttackBonus = GetUpgradeAttackBonus();
        int upgradeHpBonus = GetUpgradeHpBonus();
        double upgradeRegenBonus = GetUpgradeRegenBonus();

        int totalAttackBonus = currentAttack - BaseAttack;
        int totalShieldBonus = currentShield - BaseShield;
        int totalHpBonus = currentMaxHp - BaseMaxHp;
        double totalRegenBonus = currentRegen - BaseRegen;
        int totalSpeedBonus = currentSpeed - BaseSpeed;

        CreateBasic($"공격력 : {currentAttack}", $"(기본 공격력 : {BaseAttack})");
        CreateBasic($"방어력 : {currentShield}%", $"(기본 방어력 : {BaseShield}%)");
        CreateBasic($"최대 체력 : {currentMaxHp:N0}", $"(기본 최대 체력 : {BaseMaxHp:N0})");
        CreateBasic($"체력 회복량 : {currentRegen:F2} / s", $"(기본 체력 회복량 : {BaseRegen:F2} / s)");
        CreateBasic($"이동 속도 : {currentSpeed}", $"(기본 이동 속도 : {BaseSpeed})");

        CreateDetail(
            $"추가 공격력 : + {totalAttackBonus}",
            $"(아이템 + {weaponAttackBonus}) + (업그레이드 + {upgradeAttackBonus})"
        );

        CreateDetail(
            $"추가 방어력 : + {totalShieldBonus}%",
            $"(아이템 + {shieldDefenseBonus}%)"
        );

        CreateDetail(
            $"추가 최대 체력 : + {totalHpBonus:N0}",
            $"(아이템 + {armorHpBonus:N0}) + (업그레이드 + {upgradeHpBonus:N0})"
        );

        CreateDetail(
            $"추가 체력 회복량 : + {totalRegenBonus:F2} / s",
            $"(업그레이드 + {upgradeRegenBonus:F2} / s)"
        );

        CreateDetail(
            $"추가 이동 속도 : + {totalSpeedBonus}",
            $"(아이템 + {shoesSpeedBonus})"
        );
    }

    private void CreateBasic(string main, string sub)
    {
        if (basicParent == null || statBlockPrefab == null)
        {
            Debug.LogError("basicParent 또는 statBlockPrefab 이 비어있음");
            return;
        }

        StatBlockUI block = Instantiate(statBlockPrefab, basicParent);
        block.Set(main, sub);
        spawnedBlocks.Add(block);
    }

    private void CreateDetail(string main, string sub)
    {
        if (detailParent == null || statBlockPrefab == null)
        {
            Debug.LogError("detailParent 또는 statBlockPrefab 이 비어있음");
            return;
        }

        StatBlockUI block = Instantiate(statBlockPrefab, detailParent);
        block.Set(main, sub);
        spawnedBlocks.Add(block);
    }

    private void Clear()
    {
        for (int i = 0; i < spawnedBlocks.Count; i++)
        {
            if (spawnedBlocks[i] != null)
                Destroy(spawnedBlocks[i].gameObject);
        }

        spawnedBlocks.Clear();
    }

    private int GetWeaponAttackBonus()
    {
        ItemData item = EquipmentManager.Instance.GetEquippedItem(EquipmentSlotType.Weapon);
        return item != null ? item.attackPower : 0;
    }

    private int GetShieldDefenseBonus()
    {
        ItemData item = EquipmentManager.Instance.GetEquippedItem(EquipmentSlotType.Shield);
        return item != null ? item.shieldPower : 0;
    }

    private int GetArmorHpBonus()
    {
        ItemData item = EquipmentManager.Instance.GetEquippedItem(EquipmentSlotType.Armor);
        return item != null ? item.maxHpBonus : 0;
    }

    private int GetShoesSpeedBonus()
    {
        ItemData item = EquipmentManager.Instance.GetEquippedItem(EquipmentSlotType.Shoes);
        return item != null ? item.moveSpeedBonus : 0;
    }

    private int GetUpgradeAttackBonus()
    {
        return (UpgradeManager.Instance.GetCurrentLevel(UpgradeType.Attack) - 1) * 5;
    }

    private int GetUpgradeHpBonus()
    {
        return (UpgradeManager.Instance.GetCurrentLevel(UpgradeType.Hp) - 1) * 50;
    }

    private double GetUpgradeRegenBonus()
    {
        return (UpgradeManager.Instance.GetCurrentLevel(UpgradeType.Regen) - 1) * 0.01;
    }

    public void ClosePlayerInfo()
    {
        if (playerInfoUI != null)
            UIManager.Instance.ClosePanel(UIPanelType.PlayerInfo);
    }
}