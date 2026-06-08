using Photon.Pun;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// 로그인 / 회원가입 / 닉네임 / 서버 연결 UI
public class IntroAuthUI : MonoBehaviour
{
    [Header("Login UI")]
    [SerializeField] private TMP_InputField emailInput;
    [SerializeField] private TMP_InputField passwordInput;
    [SerializeField] private TMP_Text resultText;

    [Header("Buttons")]
    [SerializeField] private Button loginButton;
    [SerializeField] private Button registerButton;
    [SerializeField] private Button exitButton;
    [SerializeField] private Button startGameButton;

    [Header("Nickname UI")]
    [SerializeField] private GameObject nicknamePanel;
    [SerializeField] private TMP_InputField nicknameInput;
    [SerializeField] private Button nicknameConfirmButton;

    [Header("Scene")]
    [SerializeField] private string mainSceneName = "Main";

    private bool isLoggedIn;

    private void Start()
    {
        nicknamePanel?.SetActive(false);

        isLoggedIn = false;
        SetStartButtonInteractable(false);
        ShowMessage("이메일과 비밀번호를 입력하세요.");

        loginButton?.onClick.AddListener(OnClickLogin);
        registerButton?.onClick.AddListener(OnClickRegister);
        exitButton?.onClick.AddListener(OnClickExit);
        nicknameConfirmButton?.onClick.AddListener(OnClickConfirmNickname);
        startGameButton?.onClick.AddListener(OnClickStartGame);

        emailInput?.onSubmit.AddListener(_ => passwordInput?.Select());
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            if (emailInput != null && emailInput.isFocused)
                passwordInput?.Select();
            else if (passwordInput != null && passwordInput.isFocused)
                emailInput?.Select();
        }
    }

    // 로그인
    private async void OnClickLogin()
    {
        string email = GetEmail();
        string password = GetPassword();

        if (!ValidateEmailAndPassword(email, password))
            return;

        if (FirebaseAuthManager.Instance == null)
        {
            ShowMessage("Firebase 오류");
            return;
        }

        ShowMessage("로그인 중...");

        await FirebaseAuthManager.Instance.Login(email, password, (success, msg) =>
        {
            ShowMessage(msg);

            if (!success)
            {
                isLoggedIn = false;
                SetStartButtonInteractable(false);
                return;
            }

            isLoggedIn = true;

            // 닉네임 없으면 입력 강제
            if (!FirebaseAuthManager.Instance.HasNickname())
            {
                nicknamePanel?.SetActive(true);
                SetStartButtonInteractable(false);
                ShowMessage("닉네임을 설정해주세요.");
                return;
            }

            nicknamePanel?.SetActive(false);

            // Photon 연결 여부 판단
            SetStartButtonInteractable(msg.Contains("연결 완료"));
        });
    }

    // 회원가입
    private async void OnClickRegister()
    {
        string email = GetEmail();
        string password = GetPassword();

        if (!ValidateEmailAndPassword(email, password))
            return;

        if (FirebaseAuthManager.Instance == null)
        {
            ShowMessage("Firebase 오류");
            return;
        }

        ShowMessage("회원가입 중...");

        await FirebaseAuthManager.Instance.Register(email, password, (success, msg) =>
        {
            if (!success)
            {
                ShowMessage(msg);
                return;
            }

            isLoggedIn = true;
            nicknamePanel?.SetActive(true);

            SetStartButtonInteractable(false);
            ShowMessage("닉네임을 설정해주세요.");
        });
    }

    private void OnClickExit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    // 닉네임 설정
    private async void OnClickConfirmNickname()
    {
        if (!isLoggedIn)
        {
            ShowMessage("로그인 필요");
            return;
        }

        string nickname = GetNickname();
        if (!ValidateNickname(nickname))
            return;

        ShowMessage("닉네임 저장 중...");

        await FirebaseAuthManager.Instance.SetNickname(nickname, (success, msg) =>
        {
            if (!success)
            {
                ShowMessage(msg);
                return;
            }

            nicknamePanel?.SetActive(false);

            // 이미 Photon 연결됐으면 재연결 하지 않음
            if (PhotonNetwork.IsConnected)
            {
                ShowMessage("연결 완료");
                SetStartButtonInteractable(true);
                return;
            }

            ShowMessage("서버 연결 중...");
            SetStartButtonInteractable(false);

            string finalNickname = FirebaseAuthManager.Instance.GetNickname();

            PhotonNetworkManager.Instance.ConnectToPhoton(finalNickname, (ok, photonMsg) =>
            {
                ShowMessage(photonMsg);
                SetStartButtonInteractable(ok);
            });
        });
    }

    // 게임 시작
    private void OnClickStartGame()
    {
        if (!isLoggedIn)
        {
            ShowMessage("로그인 필요");
            return;
        }

        if (!FirebaseAuthManager.Instance.HasNickname())
        {
            ShowMessage("닉네임 필요");
            return;
        }

        if (!PhotonNetwork.IsConnected)
        {
            ShowMessage("서버 연결 중");
            return;
        }

        PhotonNetwork.LoadLevel(mainSceneName);
    }

    // -------------------------
    // Validation
    // -------------------------

    private bool ValidateEmailAndPassword(string email, string password)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            ShowMessage("이메일 입력");
            return false;
        }

        if (!IsValidEmail(email))
        {
            ShowMessage("이메일 형식 오류");
            return false;
        }

        if (string.IsNullOrWhiteSpace(password))
        {
            ShowMessage("비밀번호 입력");
            return false;
        }

        if (password.Length < 6)
        {
            ShowMessage("비밀번호 6자 이상");
            return false;
        }

        return true;
    }

    private bool ValidateNickname(string nickname)
    {
        if (string.IsNullOrWhiteSpace(nickname))
        {
            ShowMessage("닉네임 입력");
            return false;
        }

        if (nickname.Length < 2 || nickname.Length > 10)
        {
            ShowMessage("닉네임 2~10자");
            return false;
        }

        return true;
    }

    private bool IsValidEmail(string email)
    {
        const string pattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
        return Regex.IsMatch(email, pattern);
    }

    private string GetEmail() => emailInput?.text.Trim() ?? string.Empty;
    private string GetPassword() => passwordInput?.text.Trim() ?? string.Empty;
    private string GetNickname() => nicknameInput?.text.Trim() ?? string.Empty;

    private void ShowMessage(string message)
    {
        resultText?.SetText(message);
    }

    private void SetStartButtonInteractable(bool value)
    {
        if (startGameButton != null)
            startGameButton.interactable = value;
    }
}