using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

[RequireComponent(typeof(EnemyAI))]
// 적 위치, 회전, 이동 속도 네트워크 동기화 담당
public class EnemyAINetworkSync : MonoBehaviour, IOnEventCallback
{
    private const byte PositionSyncEvent = 70;
    private const float SyncInterval = 0.05f;

    private EnemyAI enemyAI;
    private float syncTimer;

    private void Awake()
    {
        enemyAI = GetComponent<EnemyAI>();
    }

    private void OnEnable()
    {
        PhotonNetwork.AddCallbackTarget(this);
    }

    private void OnDisable()
    {
        PhotonNetwork.RemoveCallbackTarget(this);
    }

    // 네트워크 이벤트 수신
    public void OnEvent(EventData photonEvent)
    {
        if (!enemyAI.IsInitialized) return;
        if (photonEvent.Code != PositionSyncEvent) return;

        object[] data = photonEvent.CustomData as object[];
        if (data == null || data.Length < 4) return;

        int uniqueId = (int)data[0];
        if (uniqueId != enemyAI.UniqueId) return;

        Vector3 networkPosition = (Vector3)data[1];
        Quaternion networkRotation = (Quaternion)data[2];
        float networkMoveSpeed = (float)data[3];

        enemyAI.ApplyNetworkState(networkPosition, networkRotation, networkMoveSpeed);
    }

    // 현재 상태 전송
    public void TrySyncState(Vector3 position, Quaternion rotation, float moveSpeed)
    {
        if (!PhotonNetwork.IsMasterClient) return;
        if (!enemyAI.IsInitialized) return;

        syncTimer -= Time.deltaTime;
        if (syncTimer > 0f) return;

        syncTimer = SyncInterval;

        object[] data =
        {
            enemyAI.UniqueId,
            position,
            rotation,
            moveSpeed
        };

        RaiseEventOptions options = new RaiseEventOptions
        {
            Receivers = ReceiverGroup.Others
        };

        PhotonNetwork.RaiseEvent(PositionSyncEvent, data, options, SendOptions.SendUnreliable);
    }
}