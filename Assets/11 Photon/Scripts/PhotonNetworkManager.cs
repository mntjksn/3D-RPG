using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class PhotonNetworkManager : MonoBehaviourPunCallbacks
{
    public static PhotonNetworkManager Instance { get; private set; }

    private System.Action<bool, string> _connectCallback;
    private bool _pendingSuccess = false;
    private string _pendingMsg = "";

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            // 씬 전환해도 연결 유지
            PhotonNetwork.AutomaticallySyncScene = true;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Update()
    {
        if (_pendingSuccess && _connectCallback != null)
        {
            _pendingSuccess = false;
            var cb = _connectCallback;
            _connectCallback = null;
            cb.Invoke(true, _pendingMsg);
        }
    }

    public void ConnectToPhoton(string nickname, System.Action<bool, string> callback)
    {
        _connectCallback = callback;

        PhotonNetwork.NickName = nickname;
        PhotonNetwork.GameVersion = "1.0";
        PhotonNetwork.ConnectUsingSettings();

        Debug.Log($"Photon 연결 시도 중... NickName: {nickname}");
    }

    public override void OnConnectedToMaster()
    {
        Debug.Log("Photon 마스터 서버 연결 완료");
        // JoinLobby 대신 바로 방 입장
        _pendingSuccess = true;
        _pendingMsg = "멀티플레이 서버 연결 완료!";
    }

    public override void OnJoinedLobby()
    {
        Debug.Log("로비 입장 완료");
        _pendingSuccess = true;
        _pendingMsg = "멀티플레이 서버 연결 완료!";
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        Debug.LogWarning($"Photon 연결 끊김: {cause}");
        _pendingSuccess = false;
        var cb = _connectCallback;
        _connectCallback = null;
        cb?.Invoke(false, $"서버 연결 실패: {cause}");
    }
}