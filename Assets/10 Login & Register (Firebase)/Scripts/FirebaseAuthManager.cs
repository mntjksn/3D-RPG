using UnityEngine;
using Firebase;
using Firebase.Auth;
using System;
using System.Threading.Tasks;

public class FirebaseAuthManager : MonoBehaviour
{
    public static FirebaseAuthManager Instance { get; private set; }

    private FirebaseAuth auth;
    private FirebaseUser user;

    public bool IsInitialized { get; private set; }
    public FirebaseUser CurrentUser => user;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeFirebase();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private async void InitializeFirebase()
    {
        try
        {
            var status = await FirebaseApp.CheckAndFixDependenciesAsync();

            if (status == DependencyStatus.Available)
            {
                auth = FirebaseAuth.DefaultInstance;
                user = auth.CurrentUser;
                IsInitialized = true;
                Debug.Log("Firebase 초기화 완료");
            }
            else
            {
                Debug.LogError($"Firebase 초기화 실패: {status}");
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Firebase 초기화 예외: {e}");
        }
    }

    public async Task Register(string email, string password, Action<bool, string> callback)
    {
        if (!IsInitialized)
        {
            callback?.Invoke(false, "Firebase가 아직 초기화되지 않았습니다. 잠시 후 다시 시도해주세요.");
            return;
        }

        try
        {
            var result = await auth.CreateUserWithEmailAndPasswordAsync(email, password);
            user = result.User;
            callback?.Invoke(true, "회원가입이 완료되었습니다.");
        }
        catch (Exception e)
        {
            callback?.Invoke(false, GetFirebaseAuthErrorMessage(e));
        }
    }

    public async Task Login(string email, string password, Action<bool, string> callback)
    {
        if (!IsInitialized)
        {
            callback?.Invoke(false, "Firebase가 아직 초기화되지 않았습니다. 잠시 후 다시 시도해주세요.");
            return;
        }

        try
        {
            var result = await auth.SignInWithEmailAndPasswordAsync(email, password);
            user = result.User;
            callback?.Invoke(true, "로그인에 성공했습니다.");
        }
        catch (Exception e)
        {
            callback?.Invoke(false, GetFirebaseAuthErrorMessage(e));
        }
    }

    public async Task SetNickname(string nickname, Action<bool, string> callback)
    {
        if (!IsInitialized)
        {
            callback?.Invoke(false, "Firebase가 아직 초기화되지 않았습니다.");
            return;
        }

        if (user == null)
        {
            callback?.Invoke(false, "로그인된 계정이 없습니다.");
            return;
        }

        try
        {
            UserProfile profile = new UserProfile
            {
                DisplayName = nickname
            };

            await user.UpdateUserProfileAsync(profile);

            // 최신 정보 다시 반영
            await user.ReloadAsync();
            user = auth.CurrentUser;

            callback?.Invoke(true, "닉네임 설정이 완료되었습니다.");
        }
        catch (Exception e)
        {
            Debug.LogError($"닉네임 저장 실패: {e}");
            callback?.Invoke(false, "닉네임 저장 중 오류가 발생했습니다.");
        }
    }

    public string GetNickname()
    {
        if (user == null)
            return string.Empty;

        return string.IsNullOrWhiteSpace(user.DisplayName) ? string.Empty : user.DisplayName;
    }

    public bool HasNickname()
    {
        return !string.IsNullOrWhiteSpace(GetNickname());
    }

    public void Logout()
    {
        if (auth == null)
            return;

        auth.SignOut();
        user = null;
    }

    private string GetFirebaseAuthErrorMessage(Exception exception)
    {
        if (exception is AggregateException aggregate && aggregate.InnerExceptions.Count > 0)
        {
            exception = aggregate.InnerExceptions[0];
        }

        if (exception is FirebaseException firebaseEx)
        {
            AuthError errorCode = (AuthError)firebaseEx.ErrorCode;

            switch (errorCode)
            {
                case AuthError.MissingEmail:
                    return "이메일을 입력해주세요.";

                case AuthError.InvalidEmail:
                    return "올바른 이메일 형식이 아닙니다.";

                case AuthError.MissingPassword:
                    return "비밀번호를 입력해주세요.";

                case AuthError.WeakPassword:
                    return "비밀번호는 6자 이상으로 입력해주세요.";

                case AuthError.EmailAlreadyInUse:
                    return "이미 가입된 이메일입니다.";

                case AuthError.AccountExistsWithDifferentCredentials:
                    return "다른 로그인 방식으로 이미 가입된 계정입니다.";

                case AuthError.WrongPassword:
                    return "비밀번호가 올바르지 않습니다.";

                case AuthError.UserNotFound:
                    return "가입되지 않은 계정입니다.";

                case AuthError.UserDisabled:
                    return "비활성화된 계정입니다.";

                case AuthError.NetworkRequestFailed:
                    return "네트워크 오류가 발생했습니다. 인터넷 연결을 확인해주세요.";

                case AuthError.TooManyRequests:
                    return "요청이 너무 많습니다. 잠시 후 다시 시도해주세요.";

                case AuthError.OperationNotAllowed:
                    return "현재 이 로그인 방식은 사용할 수 없습니다.";

                default:
                    Debug.LogError($"Firebase Auth ErrorCode: {firebaseEx.ErrorCode}, Message: {firebaseEx.Message}");
                    return "로그인 처리 중 오류가 발생했습니다.";
            }
        }

        Debug.LogError($"Unknown Auth Exception: {exception}");
        return "알 수 없는 오류가 발생했습니다.";
    }
}