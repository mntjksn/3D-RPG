using UnityEngine;

public class NPCInteraction : MonoBehaviour
{
    [Header("NPC Type")]
    [SerializeField] private NPCType npcType;

    [Header("UI")]
    [SerializeField] private GameObject interactUI;

    [Header("Panels")]
    [SerializeField] private GameObject shopPanel;
    [SerializeField] private GameObject questPanel;
    [SerializeField] private GameObject upgradePanel;

    private bool playerInRange = false;

    private void Start()
    {
        if (interactUI != null)
            interactUI.SetActive(false);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        playerInRange = true;

        if (interactUI != null)
            interactUI.SetActive(true);

        Debug.Log($"{name}: 플레이어가 범위 안에 들어옴");
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        playerInRange = false;

        if (interactUI != null)
            interactUI.SetActive(false);

        Debug.Log($"{name}: 플레이어가 범위 밖으로 나감");
    }

    private void Update()
    {
        if (!playerInRange)
            return;

        if (Input.GetKeyDown(KeyCode.F))
            Interact();
    }

    private void Interact()
    {
        switch (npcType)
        {
            case NPCType.Shop:
                OpenShop();
                break;

            case NPCType.Quest:
                OpenQuest();
                break;

            case NPCType.Upgrade:
                OpenUpgrade();
                break;
        }
    }

    private void OpenShop()
    {
        Debug.Log($"{name}: 상점 열기");

        if (shopPanel != null)
            shopPanel.SetActive(true);
    }

    private void OpenQuest()
    {
        Debug.Log($"{name}: 퀘스트 열기");

        if (questPanel != null)
            questPanel.SetActive(true);
    }

    private void OpenUpgrade()
    {
        Debug.Log($"{name}: 업그레이드 열기");

        if (upgradePanel != null)
            upgradePanel.SetActive(true);
    }
}