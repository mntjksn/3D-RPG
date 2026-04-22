using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

[RequireComponent(typeof(EnemyAttack))]
public class EnemyAttackNetworkSync : MonoBehaviour, IOnEventCallback
{
    private const byte ATTACK_EVENT = 50;

    private EnemyAttack enemyAttack;

    private void Awake()
    {
        enemyAttack = GetComponent<EnemyAttack>();
    }

    private void OnEnable()
    {
        PhotonNetwork.AddCallbackTarget(this);
    }

    private void OnDisable()
    {
        PhotonNetwork.RemoveCallbackTarget(this);
    }

    public void BroadcastAttack()
    {
        if (!PhotonNetwork.IsMasterClient) return;
        if (!enemyAttack.IsInitialized) return;

        object[] data = new object[]
        {
            enemyAttack.UniqueId
        };

        RaiseEventOptions options = new RaiseEventOptions
        {
            Receivers = ReceiverGroup.All
        };

        PhotonNetwork.RaiseEvent(ATTACK_EVENT, data, options, SendOptions.SendReliable);
    }

    public void OnEvent(EventData photonEvent)
    {
        if (!enemyAttack.IsInitialized) return;
        if (photonEvent.Code != ATTACK_EVENT) return;

        object[] data = (object[])photonEvent.CustomData;
        int uniqueId = (int)data[0];

        if (uniqueId != enemyAttack.UniqueId) return;

        enemyAttack.PlayAttackAnimation();
    }
}