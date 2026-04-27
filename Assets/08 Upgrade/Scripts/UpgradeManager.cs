using System;
using UnityEngine;

// 업그레이드 레벨 및 재료 선택 상태 관리
public class UpgradeManager : MonoBehaviour
{
    public static UpgradeManager Instance { get; private set; }

    private UpgradeSaveData upgradeSaveData = new UpgradeSaveData();

    private ItemData selectedAttackMaterial;
    private ItemData selectedHpMaterial;
    private ItemData selectedRegenMaterial;

    public Action<UpgradeType> OnUpgradeSuccess;

    private void Awake()
    {
        // 싱글톤 설정
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    // 업그레이드 데이터 초기화
    public void InitializeUpgrade()
    {
        upgradeSaveData = new UpgradeSaveData();

        selectedAttackMaterial = null;
        selectedHpMaterial = null;
        selectedRegenMaterial = null;
    }

    // 저장 데이터 생성
    public UpgradeSaveData GetSaveData()
    {
        return new UpgradeSaveData
        {
            attackLevel = upgradeSaveData.attackLevel,
            hpLevel = upgradeSaveData.hpLevel,
            regenLevel = upgradeSaveData.regenLevel
        };
    }

    // 저장 데이터 로드
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

    // 현재 업그레이드 레벨 조회
    public int GetCurrentLevel(UpgradeType type)
    {
        switch (type)
        {
            case UpgradeType.Attack:
                return upgradeSaveData.attackLevel;

            case UpgradeType.Hp:
                return upgradeSaveData.hpLevel;

            case UpgradeType.Regen:
                return upgradeSaveData.regenLevel;

            default:
                return 1;
        }
    }

    // 업그레이드 레벨 증가
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

        SaveManager.Instance?.MarkDirty();
        OnUpgradeSuccess?.Invoke(type);
    }

    // 업그레이드 재료 선택
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

    // 선택된 재료 조회
    public ItemData GetSelectedMaterial(UpgradeType type)
    {
        switch (type)
        {
            case UpgradeType.Attack:
                return selectedAttackMaterial;

            case UpgradeType.Hp:
                return selectedHpMaterial;

            case UpgradeType.Regen:
                return selectedRegenMaterial;

            default:
                return null;
        }
    }

    // 선택된 재료 해제
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

    // 모든 선택 재료 해제
    public void ClearAllSelectedMaterials()
    {
        ClearSelectedMaterial(UpgradeType.Attack);
        ClearSelectedMaterial(UpgradeType.Hp);
        ClearSelectedMaterial(UpgradeType.Regen);
    }
}