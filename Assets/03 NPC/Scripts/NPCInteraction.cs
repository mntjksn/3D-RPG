using UnityEngine;

public class NPCInteraction : MonoBehaviour
{
    [SerializeField] private GameObject interactUI;

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
        {
            Interact();
        }
    }

    private void Interact()
    {
        Debug.Log($"{name}: 상호작용 실행");
    }
}