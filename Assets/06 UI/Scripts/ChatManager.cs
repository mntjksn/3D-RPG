using UnityEngine;
using Photon.Pun;
using TMPro;
using UnityEngine.UI;

public class ChatManager : MonoBehaviourPun
{
    public static ChatManager Instance { get; private set; }

    [Header("UI")]
    [SerializeField] private TMP_InputField inputField;
    [SerializeField] private Transform chatContent;
    [SerializeField] private GameObject chatMessagePrefab;
    [SerializeField] private ScrollRect scrollRect;

    private bool isChatFocused = false;
    private PlayerActionLock playerActionLock;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void RegisterPlayer(PlayerActionLock actionLock)
    {
        playerActionLock = actionLock;
    }

    public void OnPressEnter()
    {
        if (!isChatFocused)
            OpenChat();
        else
            SendMessage();
    }

    private void OpenChat()
    {
        isChatFocused = true;
        inputField.ActivateInputField();
        inputField.Select();

        Cursor.lockState = CursorLockMode.Confined;
        Cursor.visible = true;
        playerActionLock?.LockRecoverControls();
    }

    private void CloseChat()
    {
        isChatFocused = false;
        inputField.text = "";
        inputField.DeactivateInputField();

        // 포커스 완전히 해제
        UnityEngine.EventSystems.EventSystem.current.SetSelectedGameObject(null);

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        playerActionLock?.UnlockRecoverControls();
    }

    private void SendMessage()
    {
        string msg = inputField.text.Trim();
        if (string.IsNullOrEmpty(msg))
        {
            CloseChat();
            return;
        }

        string nickname = FirebaseAuthManager.Instance?.GetNickname() ?? "Player";
        string fullMsg = $"[{nickname}] {msg}";

        photonView.RPC("ReceiveMessage", RpcTarget.All, fullMsg);

        inputField.text = "";

        // 한글 입력 후 잔여 글자 방지
        CloseChat();
    }

    [PunRPC]
    private void ReceiveMessage(string message)
    {
        GameObject msgObj = Instantiate(chatMessagePrefab, chatContent);
        TMP_Text text = msgObj.GetComponent<TMP_Text>();
        if (text != null)
            text.text = message;

        Canvas.ForceUpdateCanvases();
        scrollRect.verticalNormalizedPosition = 0f;
    }

    public bool IsChatFocused => isChatFocused;
}