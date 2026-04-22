using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

[RequireComponent(typeof(EnemyAI))]
public class EnemyAINetworkSync : MonoBehaviour, IOnEventCallback
{
    private const byte POSITION_SYNC_EVENT = 70;
    private const float syncInterval = 0.05f;

    private EnemyAI enemyAI;
    private float syncTimer = 0f;

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

    public void OnEvent(EventData photonEvent)
    {
        if (!enemyAI.IsInitialized) return;
        if (photonEvent.Code != POSITION_SYNC_EVENT) return;

        object[] data = (object[])photonEvent.CustomData;
        int uniqueId = (int)data[0];

        if (uniqueId != enemyAI.UniqueId) return;

        Vector3 networkPosition = (Vector3)data[1];
        Quaternion networkRotation = (Quaternion)data[2];
        float networkMoveSpeed = (float)data[3];

        enemyAI.ApplyNetworkState(networkPosition, networkRotation, networkMoveSpeed);
    }

    public void TrySyncState(Vector3 position, Quaternion rotation, float moveSpeed)
    {
        if (!PhotonNetwork.IsMasterClient) return;
        if (!enemyAI.IsInitialized) return;

        syncTimer -= Time.deltaTime;
        if (syncTimer > 0f) return;

        syncTimer = syncInterval;

        object[] data = new object[]
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

        PhotonNetwork.RaiseEvent(POSITION_SYNC_EVENT, data, options, SendOptions.SendUnreliable);
    }
}