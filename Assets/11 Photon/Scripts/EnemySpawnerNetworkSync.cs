using System.Collections;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;

[RequireComponent(typeof(EnemySpawner))]
public class EnemySpawnerNetworkSync : MonoBehaviourPunCallbacks, IOnEventCallback
{
    private EnemySpawner spawner;

    private byte ActivateEventCode => (byte)(10 + spawner.SpawnerIndex * 2);
    private byte ReturnEventCode => (byte)(11 + spawner.SpawnerIndex * 2);

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

        // 처음 스폰된 적들을 참가자에게도 보내야 하니까
        // SpawnInitialEnemies 내부에서 직접 브로드캐스트를 안 하니
        // 현재 활성 상태를 전체에게 다시 뿌려줌
        BroadcastCurrentStateToOthers();
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        Debug.Log($"[Spawner_{spawner.SpawnerIndex}] 새 플레이어 입장: {newPlayer.NickName}");

        if (!PhotonNetwork.IsMasterClient)
            return;

        StartCoroutine(SendCurrentStateRoutine(newPlayer));
    }

    private IEnumerator SendCurrentStateRoutine(Player newPlayer)
    {
        Debug.Log($"[Spawner_{spawner.SpawnerIndex}] 상태 전송 시작");
        yield return new WaitForSeconds(1f);

        spawner.SendCurrentStateToPlayer(newPlayer);

        Debug.Log($"[Spawner_{spawner.SpawnerIndex}] 상태 전송 완료");
    }

    public void BroadcastActivate(int dataIndex, int poolIndex, Vector3 spawnPos, int viewId)
    {
        object[] eventData = new object[]
        {
            (int)spawner.SpawnerIndex,
            dataIndex,
            poolIndex,
            spawnPos,
            viewId
        };

        RaiseEventOptions options = new RaiseEventOptions
        {
            Receivers = ReceiverGroup.Others,
            CachingOption = EventCaching.DoNotCache
        };

        PhotonNetwork.RaiseEvent(ActivateEventCode, eventData, options, SendOptions.SendReliable);
    }

    public void BroadcastReturn(int dataIndex, int poolIndex)
    {
        object[] eventData = new object[]
        {
            (int)spawner.SpawnerIndex,
            dataIndex,
            poolIndex
        };

        RaiseEventOptions options = new RaiseEventOptions
        {
            Receivers = ReceiverGroup.Others,
            CachingOption = EventCaching.DoNotCache
        };

        PhotonNetwork.RaiseEvent(ReturnEventCode, eventData, options, SendOptions.SendReliable);
    }

    public void SendActivateToPlayer(Player player, int dataIndex, int poolIndex, Vector3 spawnPos, int viewId)
    {
        object[] eventData = new object[]
        {
            (int)spawner.SpawnerIndex,
            dataIndex,
            poolIndex,
            spawnPos,
            viewId
        };

        RaiseEventOptions options = new RaiseEventOptions
        {
            TargetActors = new int[] { player.ActorNumber }
        };

        PhotonNetwork.RaiseEvent(ActivateEventCode, eventData, options, SendOptions.SendReliable);
    }

    private void BroadcastCurrentStateToOthers()
    {
        if (!PhotonNetwork.IsMasterClient)
            return;

        foreach (EnemyData data in spawner.EnemyDatas)
        {
            if (data == null)
                continue;
        }
    }

    public void OnEvent(EventData photonEvent)
    {
        if (photonEvent.Code == ActivateEventCode)
        {
            object[] data = (object[])photonEvent.CustomData;

            int sIndex = System.Convert.ToInt32(data[0]);
            if (sIndex != spawner.SpawnerIndex)
                return;

            int dataIndex = (int)data[1];
            int poolIndex = (int)data[2];
            Vector3 spawnPos = (Vector3)data[3];
            int viewId = (int)data[4];

            Debug.Log($"[Spawner_{spawner.SpawnerIndex}] ActivateEvent 수신: poolIndex={poolIndex}");
            spawner.ActivateEnemy(dataIndex, poolIndex, spawnPos, viewId);
        }
        else if (photonEvent.Code == ReturnEventCode)
        {
            object[] data = (object[])photonEvent.CustomData;

            int sIndex = System.Convert.ToInt32(data[0]);
            if (sIndex != spawner.SpawnerIndex)
                return;

            int dataIndex = (int)data[1];
            int poolIndex = (int)data[2];

            spawner.ReturnEnemyByIndex(dataIndex, poolIndex);
        }
    }
}