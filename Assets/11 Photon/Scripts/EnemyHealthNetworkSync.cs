using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

[RequireComponent(typeof(EnemyHealth))]
// 적 체력 관련 데미지, 회복 네트워크 동기화 담당
public class EnemyHealthNetworkSync : MonoBehaviour, IOnEventCallback
{
    private const byte DamageRequestEvent = 60;
    private const byte DamageApplyEvent = 61;
    private const byte HealSyncEvent = 62;

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

    // 마스터에게 데미지 요청
    public void RequestDamage(float damage, int attackerActorNumber)
    {
        object[] data =
        {
            health.UniqueId,
            damage,
            attackerActorNumber
        };

        PhotonNetwork.RaiseEvent(
            DamageRequestEvent,
            data,
            new RaiseEventOptions { Receivers = ReceiverGroup.MasterClient },
            SendOptions.SendReliable
        );
    }

    // 전체 클라에 데미지 적용 브로드캐스트
    public void BroadcastDamage(float damage, int attackerActorNumber)
    {
        object[] data =
        {
            health.UniqueId,
            damage,
            attackerActorNumber
        };

        PhotonNetwork.RaiseEvent(
            DamageApplyEvent,
            data,
            new RaiseEventOptions { Receivers = ReceiverGroup.All },
            SendOptions.SendReliable
        );
    }

    // 다른 클라에 회복 동기화
    public void BroadcastHeal(float hp)
    {
        object[] data =
        {
            health.UniqueId,
            hp
        };

        PhotonNetwork.RaiseEvent(
            HealSyncEvent,
            data,
            new RaiseEventOptions { Receivers = ReceiverGroup.Others },
            SendOptions.SendUnreliable
        );
    }

    // 네트워크 이벤트 수신
    public void OnEvent(EventData photonEvent)
    {
        if (!health.IsInitialized) return;

        if (photonEvent.Code == DamageRequestEvent)
        {
            if (!PhotonNetwork.IsMasterClient) return;

            object[] data = photonEvent.CustomData as object[];
            if (data == null || data.Length < 3) return;

            int id = (int)data[0];
            float damage = (float)data[1];
            int attackerActorNumber = (int)data[2];

            if (id != health.UniqueId) return;

            health.TakeDamage(damage, attackerActorNumber);
            return;
        }

        if (photonEvent.Code == DamageApplyEvent)
        {
            object[] data = photonEvent.CustomData as object[];
            if (data == null || data.Length < 3) return;

            int id = (int)data[0];
            float damage = (float)data[1];
            int attackerActorNumber = (int)data[2];

            if (id != health.UniqueId) return;

            health.ApplyDamage(damage, attackerActorNumber);
            return;
        }

        if (photonEvent.Code == HealSyncEvent)
        {
            object[] data = photonEvent.CustomData as object[];
            if (data == null || data.Length < 2) return;

            int id = (int)data[0];
            float hp = (float)data[1];

            if (id != health.UniqueId) return;

            health.ApplyHeal(hp);
        }
    }
}