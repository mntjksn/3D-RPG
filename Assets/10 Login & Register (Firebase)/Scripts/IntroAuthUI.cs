using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using System.Text.RegularExpressions;

public class IntroAuthUI : MonoBehaviour
{
    [Header("Login UI")]
    [SerializeField] private TMP_InputField emailInput;
    [SerializeField] private TMP_InputField passwordInput;
    [SerializeField] private TMP_Text resultText;

    [Header("Buttons")]
    [SerializeField] private Button loginButton;
    [SerializeField] private Button registerButton;
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
        if (nicknamePanel != null)
            nicknamePanel.SetActive(false);

        isLoggedIn = false;

        SetStartButtonInteractable(false);
        ShowMessage("이메일과 비밀번호를 입력하세요.");

        if (loginButton != null)
            loginButton.onClick.AddListener(OnClickLogin);

        if (registerButton != null)
            registerButton.onClick.AddListener(OnClickRegister);

        if (nicknameConfirmButton != null)
            nicknameConfirmButton.onClick.AddListener(OnClickConfirmNickname);

        if (startGameButton != null)
            startGameButton.onClick.AddListener(OnClickStartGame);
    }

    private async void OnClickLogin()
    {
        string email = GetEmail();
        string password = GetPassword();

        if (!ValidateEmailAndPassword(email, password))
            return;

        if (FirebaseAuthManager.Instance == null)
        {
            ShowMessage("Firebase 매니저를 찾을 수 없습니다.");
            return;
        }

        ShowMessage("로그인 중...");

        await FirebaseAuthManager.Instance.Login(email, password, (success, msg) =>
        {
            if (!success)
            {
                isLoggedIn = false;
                SetStartButtonInteractable(false);
                ShowMessage(msg);
                return;
            }

            isLoggedIn = true;

            if (FirebaseAuthManager.Instance.HasNickname())
            {
                if (nicknamePanel != null)
                    nicknamePanel.SetActive(false);

                SetStartButtonInteractable(true);
                ShowMessage("로그인에 성공했습니다.");
            }
            else
            {
                if (nicknamePanel != null)
                    nicknamePanel.SetActive(true);

                SetStartButtonInteractable(false);
                ShowMessage("닉네임을 먼저 설정해주세요.");
            }
        });
    }

    private async void OnClickRegister()
    {
        string email = GetEmail();
        string password = GetPassword();

        if (!ValidateEmailAndPassword(email, password))
            return;

        if (FirebaseAuthManager.Instance == null)
        {
            ShowMessage("Firebase 매니저를 찾을 수 없습니다.");
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

            if (nicknamePanel != null)
                nicknamePanel.SetActive(true);

            SetStartButtonInteractable(false);
            ShowMessage("회원가입이 완료되었습니다. 닉네임을 설정해주세요.");
        });
    }

    private async void OnClickConfirmNickname()
    {
        if (FirebaseAuthManager.Instance == null)
        {
            ShowMessage("Firebase 매니저를 찾을 수 없습니다.");
            return;
        }

        if (!isLoggedIn)
        {
            ShowMessage("먼저 로그인 또는 회원가입을 진행해주세요.");
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

            if (nicknamePanel != null)
                nicknamePanel.SetActive(false);

            SetStartButtonInteractable(true);
            ShowMessage("닉네임 설정이 완료되었습니다.");
        });
    }

    private void OnClickStartGame()
    {
        if (!isLoggedIn)
        {
            ShowMessage("로그인 후 게임을 시작할 수 있습니다.");
            return;
        }

        if (FirebaseAuthManager.Instance == null || !FirebaseAuthManager.Instance.HasNickname())
        {
            ShowMessage("닉네임을 먼저 설정해주세요.");
            return;
        }

        SceneManager.LoadScene(mainSceneName);
    }

    private bool ValidateEmailAndPassword(string email, string password)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            ShowMessage("이메일을 입력하세요.");
            return false;
        }

        if (!IsValidEmail(email))
        {
            ShowMessage("올바른 이메일 형식으로 입력해주세요.");
            return false;
        }

        if (string.IsNullOrWhiteSpace(password))
        {
            ShowMessage("비밀번호를 입력하세요.");
            return false;
        }

        if (password.Length < 6)
        {
            ShowMessage("비밀번호는 6자 이상 입력해주세요.");
            return false;
        }

        return true;
    }

    private bool ValidateNickname(string nickname)
    {
        if (string.IsNullOrWhiteSpace(nickname))
        {
            ShowMessage("닉네임을 입력하세요.");
            return false;
        }

        if (nickname.Length < 2 || nickname.Length > 10)
        {
            ShowMessage("닉네임은 2~10자로 입력해주세요.");
            return false;
        }

        return true;
    }

    private bool IsValidEmail(string email)
    {
        const string pattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
        return Regex.IsMatch(email, pattern);
    }

    private string GetEmail()
    {
        return emailInput != null ? emailInput.text.Trim() : string.Empty;
    }

    private string GetPassword()
    {
        return passwordInput != null ? passwordInput.text.Trim() : string.Empty;
    }

    private string GetNickname()
    {
        return nicknameInput != null ? nicknameInput.text.Trim() : string.Empty;
    }

    private void ShowMessage(string message)
    {
        if (resultText != null)
            resultText.text = message;
    }

    private void SetStartButtonInteractable(bool value)
    {
        if (startGameButton != null)
            startGameButton.interactable = value;
    }
}