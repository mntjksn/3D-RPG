using UnityEngine;

public class UpgradeManager : MonoBehaviour
{
    public static UpgradeManager Instance { get; private set; }

    private UpgradeSaveData upgradeSaveData = new UpgradeSaveData();

    private ItemData selectedAttackMaterial;
    private ItemData selectedHpMaterial;
    private ItemData selectedRegenMaterial;

    public System.Action<UpgradeType> OnUpgradeSuccess;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public void InitializeUpgrade()
    {
        upgradeSaveData = new UpgradeSaveData();

        selectedAttackMaterial = null;
        selectedHpMaterial = null;
        selectedRegenMaterial = null;
    }

    public UpgradeSaveData GetSaveData()
    {
        return new UpgradeSaveData
        {
            attackLevel = upgradeSaveData.attackLevel,
            hpLevel = upgradeSaveData.hpLevel,
            regenLevel = upgradeSaveData.regenLevel
        };
    }

    public void LoadFromSaveData(UpgradeSaveData saveData)
    {
        if (saveData == null)
        {
            InitializeUpgrade();
            return;
        }

        upgradeSaveData.attackLevel = Mathf.Max(1, saveData.attackLevel);
        upgradeSaveData.hpLevel = Mathf.Max(1, saveData.hpLevel);
        upgradeSaveData.regenLevel = Mathf.Max(1, saveData.regenLevel);

        selectedAttackMaterial = null;
        selectedHpMaterial = null;
        selectedRegenMaterial = null;
    }

    public int GetCurrentLevel(UpgradeType type)
    {
        switch (type)
        {
            case UpgradeType.Attack: return upgradeSaveData.attackLevel;
            case UpgradeType.Hp: return upgradeSaveData.hpLevel;
            case UpgradeType.Regen: return upgradeSaveData.regenLevel;
            default: return 1;
        }
    }

    public void AddLevel(UpgradeType type)
    {
        switch (type)
        {
            case UpgradeType.Attack:
                upgradeSaveData.attackLevel++;
                break;
            case UpgradeType.Hp:
                upgradeSaveData.hpLevel++;
                break;
            case UpgradeType.Regen:
                upgradeSaveData.regenLevel++;
                break;
        }
    }

    public void SetSelectedMaterial(UpgradeType type, ItemData itemData)
    {
        if (itemData == null)
            return;

        switch (type)
        {
            case UpgradeType.Attack:
                selectedAttackMaterial = itemData;
                break;
            case UpgradeType.Hp:
                selectedHpMaterial = itemData;
                break;
            case UpgradeType.Regen:
                selectedRegenMaterial = itemData;
                break;
        }
    }

    public ItemData GetSelectedMaterial(UpgradeType type)
    {
        switch (type)
        {
            case UpgradeType.Attack: return selectedAttackMaterial;
            case UpgradeType.Hp: return selectedHpMaterial;
            case UpgradeType.Regen: return selectedRegenMaterial;
            default: return null;
        }
    }

    public void ClearSelectedMaterial(UpgradeType type)
    {
        switch (type)
        {
            case UpgradeType.Attack:
                selectedAttackMaterial = null;
                break;
            case UpgradeType.Hp:
                selectedHpMaterial = null;
                break;
            case UpgradeType.Regen:
                selectedRegenMaterial = null;
                break;
        }
    }

    public void ClearAllSelectedMaterials()
    {
        ClearSelectedMaterial(UpgradeType.Attack);
        ClearSelectedMaterial(UpgradeType.Hp);
        ClearSelectedMaterial(UpgradeType.Regen);
    }
}