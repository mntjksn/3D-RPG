using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

[RequireComponent(typeof(EnemyAttack))]
// 적 공격 애니메이션 네트워크 동기화 담당
public class EnemyAttackNetworkSync : MonoBehaviour, IOnEventCallback
{
    private const byte AttackEvent = 50;

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

    // 공격 이벤트 전송
    public void BroadcastAttack()
    {
        if (!PhotonNetwork.IsMasterClient) return;
        if (!enemyAttack.IsInitialized) return;

        object[] data =
        {
            enemyAttack.UniqueId
        };

        RaiseEventOptions options = new RaiseEventOptions
        {
            Receivers = ReceiverGroup.All
        };

        PhotonNetwork.RaiseEvent(AttackEvent, data, options, SendOptions.SendReliable);
    }

    // 공격 이벤트 수신
    public void OnEvent(EventData photonEvent)
    {
        if (!enemyAttack.IsInitialized) return;
        if (photonEvent.Code != AttackEvent) return;

        object[] data = photonEvent.CustomData as object[];
        if (data == null || data.Length < 1) return;

        int uniqueId = (int)data[0];
        if (uniqueId != enemyAttack.UniqueId) return;

        enemyAttack.PlayAttackAnimation();
    }
}