using System;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

// Photon 서버 연결 및 연결 상태 콜백 관리
public class PhotonNetworkManager : MonoBehaviourPunCallbacks
{
    public static PhotonNetworkManager Instance { get; private set; }

    private Action<bool, string> connectCallback;
    private bool isConnecting;

    private void Awake()
    {
        // 싱글톤 설정
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        // 씬 전환 시 자동 동기화
        PhotonNetwork.AutomaticallySyncScene = true;
    }

    // Photon 연결 시도
    public void ConnectToPhoton(string nickname, Action<bool, string> callback)
    {
        connectCallback = callback;

        if (PhotonNetwork.IsConnectedAndReady)
        {
            connectCallback?.Invoke(true, "멀티플레이 서버 연결 완료!");
            connectCallback = null;
            return;
        }

        if (PhotonNetwork.IsConnected)
        {
            connectCallback?.Invoke(true, "멀티플레이 서버 연결 완료!");
            connectCallback = null;
            return;
        }

        if (isConnecting)
            return;

        isConnecting = true;

        PhotonNetwork.NickName = nickname;
        PhotonNetwork.GameVersion = "1.0";
        PhotonNetwork.ConnectUsingSettings();
    }

    public override void OnConnectedToMaster()
    {
        isConnecting = false;

        connectCallback?.Invoke(true, "멀티플레이 서버 연결 완료!");
        connectCallback = null;
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        isConnecting = false;

        connectCallback?.Invoke(false, $"서버 연결 실패: {cause}");
        connectCallback = null;
    }
}