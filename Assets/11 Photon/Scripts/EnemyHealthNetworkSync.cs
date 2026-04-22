using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

[RequireComponent(typeof(EnemyHealth))]
public class EnemyHealthNetworkSync : MonoBehaviour, IOnEventCallback
{
    private const byte DAMAGE_REQUEST_EVENT = 60;
    private const byte DAMAGE_APPLY_EVENT = 61;
    private const byte HEAL_SYNC_EVENT = 62;

    private EnemyHealth health;

    private void Awake()
    {
        health = GetComponent<EnemyHealth>();
    }

    private void OnEnable()
    {
        PhotonNetwork.AddCallbackTarget(this);
    }

    private void OnDisable()
    {
        PhotonNetwork.RemoveCallbackTarget(this);
    }

    public void RequestDamage(float damage, int attackerActorNumber)
    {
        object[] data = new object[]
        {
            health.UniqueId,
            damage,
            attackerActorNumber
        };

        PhotonNetwork.RaiseEvent(
            DAMAGE_REQUEST_EVENT,
            data,
            new RaiseEventOptions { Receivers = ReceiverGroup.MasterClient },
            SendOptions.SendReliable
        );
    }

    public void BroadcastDamage(float damage, int attackerActorNumber)
    {
        object[] data = new object[]
        {
            health.UniqueId,
            damage,
            attackerActorNumber
        };

        PhotonNetwork.RaiseEvent(
            DAMAGE_APPLY_EVENT,
            data,
            new RaiseEventOptions { Receivers = ReceiverGroup.All },
            SendOptions.SendReliable
        );
    }

    public void BroadcastHeal(float hp)
    {
        object[] data = new object[]
        {
            health.UniqueId,
            hp
        };

        PhotonNetwork.RaiseEvent(
            HEAL_SYNC_EVENT,
            data,
            new RaiseEventOptions { Receivers = ReceiverGroup.Others },
            SendOptions.SendUnreliable
        );
    }

    public void OnEvent(EventData photonEvent)
    {
        if (!health.IsInitialized) return;

        if (photonEvent.Code == DAMAGE_REQUEST_EVENT)
        {
            if (!PhotonNetwork.IsMasterClient) return;

            object[] data = (object[])photonEvent.CustomData;
            int id = (int)data[0];
            float damage = (float)data[1];
            int attackerActorNumber = (int)data[2];

            if (id != health.UniqueId) return;

            health.TakeDamage(damage, attackerActorNumber);
        }
        else if (photonEvent.Code == DAMAGE_APPLY_EVENT)
        {
            object[] data = (object[])photonEvent.CustomData;
            int id = (int)data[0];
            float damage = (float)data[1];
            int attackerActorNumber = (int)data[2];

            if (id != health.UniqueId) return;

            health.ApplyDamage(damage, attackerActorNumber);
        }
        else if (photonEvent.Code == HEAL_SYNC_EVENT)
        {
            object[] data = (object[])photonEvent.CustomData;
            int id = (int)data[0];
            float hp = (float)data[1];

            if (id != health.UniqueId) return;

            health.ApplyHeal(hp);
        }
    }
}