using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Firebase;
using Firebase.Auth;
using Firebase.Database;
using UnityEngine;

// Firebase 인증 및 온라인 상태 관리
public class FirebaseAuthManager : MonoBehaviour
{
    public static FirebaseAuthManager Instance { get; private set; }

    private FirebaseAuth auth;
    private FirebaseUser user;
    private DatabaseReference dbRef;

    private Coroutine heartbeatCoroutine;
    private string currentSessionId;

    private const float HeartbeatInterval = 10f;
    private const long OnlineTimeoutMs = 30000;

    public bool IsInitialized { get; private set; }
    public FirebaseUser CurrentUser => user;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        InitializeFirebase();
    }

    private void OnApplicationQuit()
    {
        SetOnlineStatus(false);
    }

    // Firebase 초기화
    private async void InitializeFirebase()
    {
        try
        {
            var status = await FirebaseApp.CheckAndFixDependenciesAsync();

            if (status != DependencyStatus.Available)
                return;

            auth = FirebaseAuth.DefaultInstance;
            user = auth.CurrentUser;
            dbRef = FirebaseDatabase.DefaultInstance.RootReference;
            IsInitialized = true;
        }
        catch (Exception e)
        {
            Debug.LogError($"Firebase Init Error: {e}");
        }
    }

    // 회원가입
    public async Task Register(string email, string password, Action<bool, string> callback)
    {
        if (!IsInitialized)
        {
            callback?.Invoke(false, "잠시 후 다시 시도해주세요.");
            return;
        }

        try
        {
            var result = await auth.CreateUserWithEmailAndPasswordAsync(email, password);
            user = result.User;
            callback?.Invoke(true, "회원가입 완료");
        }
        catch (Exception e)
        {
            callback?.Invoke(false, GetFirebaseAuthErrorMessage(e));
        }
    }

    // 로그인
    public async Task Login(string email, string password, Action<bool, string> callback)
    {
        if (!IsInitialized)
        {
            callback?.Invoke(false, "잠시 후 다시 시도해주세요.");
            return;
        }

        try
        {
            var result = await auth.SignInWithEmailAndPasswordAsync(email, password);
            user = result.User;

            bool isAlreadyOnline = await IsUserOnline(user.UserId);
            if (isAlreadyOnline)
            {
                auth.SignOut();
                user = null;
                callback?.Invoke(false, "이미 로그인 중입니다.");
                return;
            }

            currentSessionId = Guid.NewGuid().ToString("N");
            await SetOnlineStatusAsync(true);

            StartHeartbeat();

            string nickname = string.IsNullOrWhiteSpace(user.DisplayName)
                ? user.Email.Split('@')[0]
                : user.DisplayName;

            PhotonNetworkManager.Instance.ConnectToPhoton(nickname, callback);
        }
        catch (Exception e)
        {
            callback?.Invoke(false, GetFirebaseAuthErrorMessage(e));
        }
    }

    // 현재 로그인 상태 확인
    private async Task<bool> IsUserOnline(string uid)
    {
        if (dbRef == null || string.IsNullOrEmpty(uid))
            return false;

        try
        {
            var snapshot = await dbRef.Child("online_users").Child(uid).GetValueAsync();
            if (!snapshot.Exists || snapshot.Value == null)
                return false;

            object rawValue = snapshot.Value;

            if (rawValue is bool boolValue)
                return boolValue;

            if (rawValue is Dictionary<string, object> dict)
            {
                bool isOnline = dict.TryGetValue("isOnline", out object onlineObj)
                    && ConvertToBool(onlineObj);

                if (!isOnline)
                    return false;

                long lastActiveAt = dict.TryGetValue("lastActiveAt", out object timeObj)
                    ? ConvertToLong(timeObj)
                    : 0;

                long nowMs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                return nowMs - lastActiveAt <= OnlineTimeoutMs;
            }

            return false;
        }
        catch
        {
            return false;
        }
    }

    // 온라인 상태 저장
    private async Task SetOnlineStatusAsync(bool isOnline)
    {
        if (user == null || dbRef == null)
            return;

        try
        {
            var userRef = dbRef.Child("online_users").Child(user.UserId);

            Dictionary<string, object> data = new Dictionary<string, object>
            {
                { "isOnline", isOnline },
                { "sessionId", isOnline ? currentSessionId : string.Empty },
                { "lastActiveAt", DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() }
            };

            await userRef.SetValueAsync(data);

            if (isOnline)
            {
                // OnDisconnect는 SetValue 사용 (Unity Firebase SDK)
                Dictionary<string, object> offlineData = new Dictionary<string, object>
                {
                    { "isOnline", false },
                    { "sessionId", string.Empty },
                    { "lastActiveAt", DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() }
                };

                await userRef.OnDisconnect().SetValue(offlineData);
            }
            else
            {
                await userRef.OnDisconnect().Cancel();
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Online Status Error: {e}");
        }
    }

    private async void SetOnlineStatus(bool isOnline)
    {
        await SetOnlineStatusAsync(isOnline);
    }

    // heartbeat 시작
    private void StartHeartbeat()
    {
        StopHeartbeat();
        heartbeatCoroutine = StartCoroutine(HeartbeatRoutine());
    }

    // heartbeat 중지
    private void StopHeartbeat()
    {
        if (heartbeatCoroutine == null)
            return;

        StopCoroutine(heartbeatCoroutine);
        heartbeatCoroutine = null;
    }

    private IEnumerator HeartbeatRoutine()
    {
        while (user != null)
        {
            _ = SetOnlineStatusAsync(true);
            yield return new WaitForSecondsRealtime(HeartbeatInterval);
        }
    }

    // 로그아웃
    public void Logout()
    {
        if (auth == null)
            return;

        StopHeartbeat();
        SetOnlineStatus(false);

        auth.SignOut();
        user = null;
        currentSessionId = null;
    }

    // 닉네임 설정
    public async Task SetNickname(string nickname, Action<bool, string> callback)
    {
        if (user == null || dbRef == null)
        {
            callback?.Invoke(false, "초기화 오류");
            return;
        }

        try
        {
            var snapshot = await dbRef.Child("nicknames").Child(nickname).GetValueAsync();
            if (snapshot.Exists)
            {
                callback?.Invoke(false, "이미 사용 중인 닉네임입니다.");
                return;
            }

            UserProfile profile = new UserProfile { DisplayName = nickname };
            await user.UpdateUserProfileAsync(profile);
            await user.ReloadAsync();

            await dbRef.Child("nicknames").Child(nickname).SetValueAsync(user.UserId);

            user = auth.CurrentUser;
            callback?.Invoke(true, "닉네임 설정 완료");
        }
        catch (Exception e)
        {
            Debug.LogError($"SetNickname Error: {e}");
            callback?.Invoke(false, "닉네임 저장 실패: " + e.Message);
        }
    }

    public string GetNickname()
    {
        if (user == null)
            return string.Empty;

        return string.IsNullOrWhiteSpace(user.DisplayName) ? string.Empty : user.DisplayName;
    }

    public bool HasNickname() => !string.IsNullOrWhiteSpace(GetNickname());

    private bool ConvertToBool(object value)
    {
        if (value == null) return false;
        if (value is bool b) return b;
        if (bool.TryParse(value.ToString(), out bool parsed)) return parsed;
        return false;
    }

    private long ConvertToLong(object value)
    {
        if (value == null) return 0;
        if (value is long l) return l;
        if (value is int i) return i;
        if (long.TryParse(value.ToString(), out long parsed)) return parsed;
        return 0;
    }

    // Firebase 에러 메시지 변환
    private string GetFirebaseAuthErrorMessage(Exception exception)
    {
        if (exception is AggregateException aggregate && aggregate.InnerExceptions.Count > 0)
            exception = aggregate.InnerExceptions[0];

        if (exception is FirebaseException firebaseEx)
        {
            AuthError code = (AuthError)firebaseEx.ErrorCode;

            switch (code)
            {
                case AuthError.MissingEmail: return "이메일 입력";
                case AuthError.InvalidEmail: return "이메일 형식 오류";
                case AuthError.MissingPassword: return "비밀번호 입력";
                case AuthError.WeakPassword: return "비밀번호 6자 이상";
                case AuthError.EmailAlreadyInUse: return "이미 가입된 이메일";
                case AuthError.WrongPassword: return "비밀번호 오류";
                case AuthError.UserNotFound: return "계정 없음";
                case AuthError.NetworkRequestFailed: return "네트워크 오류";
                default: return "로그인 오류";
            }
        }

        return "알 수 없는 오류";
    }
}