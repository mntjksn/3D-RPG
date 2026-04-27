using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// 플레이어 스탯 UI 생성 및 갱신 담당
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

    private readonly List<StatBlockUI> spawnedBlocks = new();

    private const int BaseAttack = 10;
    private const int BaseShield = 20;
    private const int BaseMaxHp = 100;
    private const double BaseRegen = 0.01;
    private const int BaseSpeed = 8;

    private PlayerStat playerStat;

    private void Start()
    {
        BindButtons();
    }

    private void OnEnable()
    {
        StartCoroutine(InitAndRefresh());
    }

    private IEnumerator InitAndRefresh()
    {
        // PlayerStat 준비될 때까지 대기
        yield return new WaitUntil(() =>
            PlayerManager.Instance != null &&
            PlayerManager.Instance.Stat != null);

        playerStat = PlayerManager.Instance.Stat;
        Refresh();
    }

    // 버튼 연결
    private void BindButtons()
    {
        closeButton?.onClick.AddListener(ClosePlayerInfo);
    }

    // UI 전체 갱신
    public void Refresh()
    {
        if (playerStat == null) return;

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

    // 기본 스탯 UI 생성
    private void CreateBasic(string main, string sub)
    {
        if (basicParent == null || statBlockPrefab == null) return;

        StatBlockUI block = Instantiate(statBlockPrefab, basicParent);
        block.Set(main, sub);
        spawnedBlocks.Add(block);
    }

    // 상세 스탯 UI 생성
    private void CreateDetail(string main, string sub)
    {
        if (detailParent == null || statBlockPrefab == null) return;

        StatBlockUI block = Instantiate(statBlockPrefab, detailParent);
        block.Set(main, sub);
        spawnedBlocks.Add(block);
    }

    // 생성된 UI 제거
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
        ItemData item = EquipmentManager.Instance?.GetEquippedItem(EquipmentSlotType.Weapon);
        return item != null ? item.attackPower : 0;
    }

    private int GetShieldDefenseBonus()
    {
        ItemData item = EquipmentManager.Instance?.GetEquippedItem(EquipmentSlotType.Shield);
        return item != null ? item.shieldPower : 0;
    }

    private int GetArmorHpBonus()
    {
        ItemData item = EquipmentManager.Instance?.GetEquippedItem(EquipmentSlotType.Armor);
        return item != null ? item.maxHpBonus : 0;
    }

    private int GetShoesSpeedBonus()
    {
        ItemData item = EquipmentManager.Instance?.GetEquippedItem(EquipmentSlotType.Shoes);
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

    // UI 닫기
    public void ClosePlayerInfo()
    {
        UIManager.Instance?.ClosePanel(UIPanelType.PlayerInfo);
    }
}