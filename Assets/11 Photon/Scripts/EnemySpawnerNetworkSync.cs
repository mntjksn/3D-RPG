using System.Collections;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;

[RequireComponent(typeof(EnemySpawner))]
// 적 스포너 상태 네트워크 동기화 담당
public class EnemySpawnerNetworkSync : MonoBehaviourPunCallbacks, IOnEventCallback
{
    private const byte BaseActivateEventCode = 10;
    private const byte BaseReturnEventCode = 11;

    private EnemySpawner spawner;

    private byte ActivateEventCode => (byte)(BaseActivateEventCode + spawner.SpawnerIndex * 2);
    private byte ReturnEventCode => (byte)(BaseReturnEventCode + spawner.SpawnerIndex * 2);

    private void Awake()
    {
        spawner = GetComponent<EnemySpawner>();
    }

    public override void OnEnable()
    {
        base.OnEnable();
        PhotonNetwork.AddCallbackTarget(this);
    }

    public override void OnDisable()
    {
        base.OnDisable();
        PhotonNetwork.RemoveCallbackTarget(this);
    }

    private void Start()
    {
        StartCoroutine(WaitForRoomAndSpawn());
    }

    // 방 입장 후 초기 스폰
    private IEnumerator WaitForRoomAndSpawn()
    {
        yield return new WaitUntil(() => PhotonNetwork.InRoom);

        if (!PhotonNetwork.IsMasterClient)
            yield break;

        yield return new WaitUntil(() =>
            PlayerManager.Instance != null &&
            PlayerManager.Instance.Stat != null);

        yield return null;

        spawner.SpawnInitialEnemiesAndBroadcast();
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        if (!PhotonNetwork.IsMasterClient)
            return;

        StartCoroutine(SendCurrentStateRoutine(newPlayer));
    }

    // 새 플레이어에게 현재 상태 전송
    private IEnumerator SendCurrentStateRoutine(Player newPlayer)
    {
        yield return new WaitForSeconds(1f);
        spawner.SendCurrentStateToPlayer(newPlayer);
    }

    // 적 생성 브로드캐스트
    public void BroadcastActivate(int dataIndex, int poolIndex, Vector3 spawnPos, int viewId)
    {
        object[] eventData =
        {
            spawner.SpawnerIndex,
            dataIndex,
            poolIndex,
            spawnPos,
            viewId
        };

        PhotonNetwork.RaiseEvent(
            ActivateEventCode,
            eventData,
            new RaiseEventOptions
            {
                Receivers = ReceiverGroup.Others,
                CachingOption = EventCaching.DoNotCache
            },
            SendOptions.SendReliable
        );
    }

    // 적 반환 브로드캐스트
    public void BroadcastReturn(int dataIndex, int poolIndex)
    {
        object[] eventData =
        {
            spawner.SpawnerIndex,
            dataIndex,
            poolIndex
        };

        PhotonNetwork.RaiseEvent(
            ReturnEventCode,
            eventData,
            new RaiseEventOptions
            {
                Receivers = ReceiverGroup.Others,
                CachingOption = EventCaching.DoNotCache
            },
            SendOptions.SendReliable
        );
    }

    // 특정 플레이어에게 생성 정보 전송
    public void SendActivateToPlayer(Player player, int dataIndex, int poolIndex, Vector3 spawnPos, int viewId)
    {
        object[] eventData =
        {
            spawner.SpawnerIndex,
            dataIndex,
            poolIndex,
            spawnPos,
            viewId
        };

        PhotonNetwork.RaiseEvent(
            ActivateEventCode,
            eventData,
            new RaiseEventOptions
            {
                TargetActors = new[] { player.ActorNumber }
            },
            SendOptions.SendReliable
        );
    }

    // 이벤트 수신
    public void OnEvent(EventData photonEvent)
    {
        if (!gameObject.activeInHierarchy)
            return;

        if (photonEvent.Code == ActivateEventCode)
        {
            object[] data = photonEvent.CustomData as object[];
            if (data == null || data.Length < 5)
                return;

            int spawnerIndex = System.Convert.ToInt32(data[0]);
            if (spawnerIndex != spawner.SpawnerIndex)
                return;

            int dataIndex = System.Convert.ToInt32(data[1]);
            int poolIndex = System.Convert.ToInt32(data[2]);
            Vector3 spawnPos = (Vector3)data[3];
            int viewId = System.Convert.ToInt32(data[4]);

            spawner.ActivateEnemy(dataIndex, poolIndex, spawnPos, viewId);
            return;
        }

        if (photonEvent.Code == ReturnEventCode)
        {
            object[] data = photonEvent.CustomData as object[];
            if (data == null || data.Length < 3)
                return;

            int spawnerIndex = System.Convert.ToInt32(data[0]);
            if (spawnerIndex != spawner.SpawnerIndex)
                return;

            int dataIndex = System.Convert.ToInt32(data[1]);
            int poolIndex = System.Convert.ToInt32(data[2]);

            spawner.ReturnEnemyByIndex(dataIndex, poolIndex);
        }
    }
}