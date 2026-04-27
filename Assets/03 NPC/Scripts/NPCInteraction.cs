using UnityEngine;

// NPC 상호작용 - 범위 진입 시 UI 표시, F 키로 패널 오픈
public class NPCInteraction : MonoBehaviour
{
    [Header("Open Panel")]
    [SerializeField] private UIPanelType panelType;

    [Header("UI")]
    [SerializeField] private GameObject interactUI;

    private bool playerInRange;

    private void Start()
    {
        // 시작 시 UI 숨김
        interactUI?.SetActive(false);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        playerInRange = true;
        interactUI?.SetActive(true);
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        playerInRange = false;
        interactUI?.SetActive(false);
    }

    private void Update()
    {
        if (!playerInRange) return;

        // F 키로 상호작용
        if (Input.GetKeyDown(KeyCode.F))
            Interact();
    }

    // UI 패널 열기 시도
    private void Interact()
    {
        if (UIManager.Instance == null) return;

        SoundManager.Instance?.PlaySFX(SfxType.Interaction);

        UIManager.Instance.TryOpenPanel(panelType);
    }
}