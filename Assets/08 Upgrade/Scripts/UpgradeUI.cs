using UnityEngine;
using UnityEngine.UI;

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
    [SerializeField] private GameObject upgradePanel;

    [Header("Data")]
    [SerializeField] private StatUpgradeData attackData;
    [SerializeField] private StatUpgradeData hpData;
    [SerializeField] private StatUpgradeData regenData;

    [Header("Player")]
    [SerializeField] private PlayerStat playerStat;
    [SerializeField] private PlayerActionLock playerActionLock;

    private void Start()
    {
        if (closeButton != null)
            closeButton.onClick.AddListener(CloseUpgrade);

        RefreshAll();
    }

    private void OnEnable()
    {
        Cursor.lockState = CursorLockMode.Confined;
        Cursor.visible = true;

        if (playerActionLock != null)
            playerActionLock.LockRecoverControls();

        UpgradeManager.Instance?.ClearAllSelectedMaterials();
        RefreshAll();
    }

    private void OnDisable()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (playerActionLock != null)
            playerActionLock.UnlockRecoverControls();
    }

    public void RefreshAll()
    {
        if (upgradeInventoryUI != null)
            upgradeInventoryUI.RefreshUI();

        RefreshSelectedSlots();
        RefreshRows();
    }

    public void OnDropMaterial(UpgradeType type, ItemData itemData)
    {
        UpgradeManager.Instance?.SetSelectedMaterial(type, itemData);
        RefreshAll();
    }

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
        {
            UpgradeManager.Instance?.ClearSelectedMaterial(type);
            Debug.Log($"{type} 업그레이드 성공");
        }
        else
        {
            Debug.Log($"{type} 업그레이드 실패");
        }

        RefreshAll();
    }

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
            case UpgradeType.Attack: return attackData;
            case UpgradeType.Hp: return hpData;
            case UpgradeType.Regen: return regenData;
            default: return null;
        }
    }

    public void CloseUpgrade()
    {
        if (upgradePanel != null)
            UIManager.Instance.ClosePanel(UIPanelType.Upgrade);
    }
}