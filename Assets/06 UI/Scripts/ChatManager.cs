using Photon.Pun;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// 채팅 입력, 전송, 메시지 표시 담당
public class ChatManager : MonoBehaviourPun
{
    public static ChatManager Instance { get; private set; }

    [Header("UI")]
    [SerializeField] private TMP_InputField inputField;
    [SerializeField] private Transform chatContent;
    [SerializeField] private GameObject chatMessagePrefab;
    [SerializeField] private ScrollRect scrollRect;

    private bool isChatFocused;
    private PlayerActionLock playerActionLock;

    public bool IsChatFocused => isChatFocused;

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

    // 플레이어 조작 잠금 대상 등록
    public void RegisterPlayer(PlayerActionLock actionLock)
    {
        playerActionLock = actionLock;
    }

    // Enter 입력 처리
    public void OnPressEnter()
    {
        if (!isChatFocused)
            OpenChat();
        else
            SendChatMessage();
    }

    // 채팅 입력 시작
    private void OpenChat()
    {
        isChatFocused = true;

        inputField?.ActivateInputField();
        inputField?.Select();

        Cursor.lockState = CursorLockMode.Confined;
        Cursor.visible = true;
        playerActionLock?.LockRecoverControls();
    }

    // 채팅 입력 종료
    private void CloseChat()
    {
        isChatFocused = false;

        if (inputField != null)
        {
            inputField.text = string.Empty;
            inputField.DeactivateInputField();
        }

        UnityEngine.EventSystems.EventSystem.current?.SetSelectedGameObject(null);

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        playerActionLock?.UnlockRecoverControls();
    }

    // 채팅 메시지 전송
    private void SendChatMessage()
    {
        if (inputField == null)
        {
            CloseChat();
            return;
        }

        string msg = inputField.text.Trim();
        if (string.IsNullOrEmpty(msg))
        {
            CloseChat();
            return;
        }

        string nickname = FirebaseAuthManager.Instance?.GetNickname() ?? "Player";
        string fullMsg = $"[{nickname}] {msg}";

        photonView.RPC(nameof(ReceiveMessage), RpcTarget.All, fullMsg);

        inputField.text = string.Empty;
        CloseChat();
    }

    // 채팅 메시지 표시
    [PunRPC]
    private void ReceiveMessage(string message)
    {
        if (chatMessagePrefab == null || chatContent == null)
            return;

        GameObject msgObj = Instantiate(chatMessagePrefab, chatContent);
        TMP_Text text = msgObj.GetComponent<TMP_Text>();
        text?.SetText(message);

        Canvas.ForceUpdateCanvases();

        if (scrollRect != null)
            scrollRect.verticalNormalizedPosition = 0f;
    }
}