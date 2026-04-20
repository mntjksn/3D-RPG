using UnityEngine;

public class NPCInteraction : MonoBehaviour
{
    [Header("Open Panel")]
    [SerializeField] private UIPanelType panelType;

    [Header("UI")]
    [SerializeField] private GameObject interactUI;

    private bool playerInRange;

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
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        playerInRange = false;

        if (interactUI != null)
            interactUI.SetActive(false);
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
        if (UIManager.Instance == null)
            return;

        SoundManager.Instance.PlaySFX(SfxType.Interaction);
        bool opened = UIManager.Instance.TryOpenPanel(panelType);

        if (!opened)
            Debug.Log("이미 다른 창이 열려 있음");
    }
}