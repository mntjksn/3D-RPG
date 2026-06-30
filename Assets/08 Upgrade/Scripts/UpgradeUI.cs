using System.Collections;
using UnityEngine;
using UnityEngine.UI;

// 업그레이드 UI 전체 갱신 및 강화 처리 담당
public class UpgradeUI : MonoBehaviour
{
    [Header("Rows")]
    [SerializeField] private UpgradeRowUI attackRowUI;
    [SerializeField] private UpgradeRowUI hpRowUI;
    [SerializeField] private UpgradeRowUI regenRowUI;

    [Header("Selected Slots")]
    [SerializeField] private UpgradeSelectedMaterialSlotUI attackSlotUI;
    [SerializeField] private UpgradeSelectedMaterialSlotUI hpSlotUI;
    [SerializeField] private UpgradeSelectedMaterialSlotUI regenSlotUI;

    [Header("Inventory UI")]
    [SerializeField] private UpgradeInventoryUI upgradeInventoryUI;

    [Header("Buttons")]
    [SerializeField] private Button closeButton;

    [Header("Data")]
    [SerializeField] private StatUpgradeData attackData;
    [SerializeField] private StatUpgradeData hpData;
    [SerializeField] private StatUpgradeData regenData;

    private PlayerStat playerStat;

    private void Start()
    {
        closeButton?.onClick.AddListener(CloseUpgrade);
        RefreshAll();
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

        UpgradeManager.Instance?.ClearAllSelectedMaterials();
        RefreshAll();
    }

    // 업그레이드 UI 전체 갱신
    public void RefreshAll()
    {
        upgradeInventoryUI?.RefreshUI();

        RefreshSelectedSlots();
        RefreshRows();
    }

    // 재료 드롭 처리
    public void OnDropMaterial(UpgradeType type, ItemData itemData)
    {
        UpgradeManager.Instance?.SetSelectedMaterial(type, itemData);
        RefreshAll();
    }

    // 업그레이드 버튼 클릭 처리
    public void OnClickUpgrade(UpgradeType type)
    {
        StatUpgradeData statData = GetStatData(type);

        bool success = UpgradeService.TryUpgrade(
            UpgradeManager.Instance,
            type,
            statData,
            playerStat
        );

        if (success)
            UpgradeManager.Instance?.ClearSelectedMaterial(type);

        RefreshAll();
    }

    // 선택 슬롯 갱신
    private void RefreshSelectedSlots()
    {
        RefreshSelectedSlot(UpgradeType.Attack, attackSlotUI);
        RefreshSelectedSlot(UpgradeType.Hp, hpSlotUI);
        RefreshSelectedSlot(UpgradeType.Regen, regenSlotUI);
    }

    private void RefreshSelectedSlot(UpgradeType type, UpgradeSelectedMaterialSlotUI slotUI)
    {
        if (slotUI == null || InventoryManager.Instance == null || UpgradeManager.Instance == null)
            return;

        ItemData itemData = UpgradeManager.Instance.GetSelectedMaterial(type);
        if (itemData == null)
        {
            slotUI.ClearSlot();
            return;
        }

        int count = InventoryManager.Instance.GetItemCount(itemData.itemId);
        if (count <= 0)
        {
            UpgradeManager.Instance.ClearSelectedMaterial(type);
            slotUI.ClearSlot();
            return;
        }

        slotUI.RefreshSlot(itemData, count);
    }

    // 업그레이드 행 갱신
    private void RefreshRows()
    {
        RefreshRow(UpgradeType.Attack, attackRowUI, attackData);
        RefreshRow(UpgradeType.Hp, hpRowUI, hpData);
        RefreshRow(UpgradeType.Regen, regenRowUI, regenData);
    }

    private void RefreshRow(UpgradeType type, UpgradeRowUI rowUI, StatUpgradeData statData)
    {
        if (rowUI == null || statData == null || UpgradeManager.Instance == null || playerStat == null)
            return;

        ItemData selectedItem = UpgradeManager.Instance.GetSelectedMaterial(type);
        int currentLevel = UpgradeManager.Instance.GetCurrentLevel(type);
        int selectedCount = 0;

        if (selectedItem != null && InventoryManager.Instance != null)
            selectedCount = InventoryManager.Instance.GetItemCount(selectedItem.itemId);

        rowUI.Refresh(statData, currentLevel, selectedItem, selectedCount, playerStat.Gold);
    }

    private StatUpgradeData GetStatData(UpgradeType type)
    {
        switch (type)
        {
            case UpgradeType.Attack:
                return attackData;

            case UpgradeType.Hp:
                return hpData;

            case UpgradeType.Regen:
                return regenData;

            default:
                return null;
        }
    }

    // 업그레이드 창 닫기
    public void CloseUpgrade()
    {
        UIManager.Instance?.ClosePanel(UIPanelType.Upgrade);
    }
}