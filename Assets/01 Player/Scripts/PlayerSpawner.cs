using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.SceneManagement;

// Photon 방 입장 및 플레이어 스폰, 이름표/HP바 캔버스 관리
public class PlayerSpawner : MonoBehaviourPunCallbacks
{
    private const string RoomName = "MainRoom";
    private const byte MaxPlayers = 10;

    [Header("Spawn Settings")]
    [SerializeField] private string playerPrefabName = "Player";
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private GameObject playerCanvasPrefab;

    [Header("Fallback Spawn")]
    [SerializeField] private Vector3 defaultSpawnPosition = new Vector3(9.63f, 1.76f, 59.84f);
    [SerializeField] private Vector3 defaultSpawnEuler = Vector3.zero;

    // 씬 전환 후에도 중복 스폰 방지
    private static bool spawned = false;

    // ActorNumber -> 캔버스 오브젝트 매핑 (퇴장 시 제거용)
    private readonly Dictionary<int, GameObject> canvasByActorNumber = new();

    private void Start()
    {
        TryEnterRoomOrConnect();
    }

    // 연결 상태에 따라 입장 또는 접속 시도
    private void TryEnterRoomOrConnect()
    {
        if (PhotonNetwork.InRoom)
        {
            SpawnPlayer();
            return;
        }

        if (PhotonNetwork.IsConnectedAndReady)
        {
            JoinMainRoom();
            return;
        }

        PhotonNetwork.ConnectUsingSettings();
    }

    private void JoinMainRoom()
    {
        RoomOptions options = new RoomOptions { MaxPlayers = MaxPlayers };
        PhotonNetwork.JoinOrCreateRoom(RoomName, options, null);
    }

    public override void OnConnectedToMaster()
    {
        if (!spawned)
            JoinMainRoom();
    }

    public override void OnJoinedRoom()
    {
        if (spawned) return;

        // 같은 닉네임 중복 접속 감지 시 강제 퇴장
        if (HasDuplicateNickname())
        {
            Debug.LogWarning("중복 로그인 감지! 접속 종료");
            PhotonNetwork.LeaveRoom();
            SceneManager.LoadScene("Intro");
            return;
        }

        Debug.Log($"방 입장 완료 - 방: {PhotonNetwork.CurrentRoom.Name}, 인원: {PhotonNetwork.CurrentRoom.PlayerCount}");

        SpawnPlayer();
        StartCoroutine(AttachCanvasToExistingPlayersRoutine());
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        Debug.LogError($"방 입장 실패: {message}");
    }

    // 새 플레이어 입장 시 캔버스 부착 및 EnemyAttack 캐시 갱신
    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        StartCoroutine(AttachCanvasToNewPlayerRoutine(newPlayer));
        RefreshEnemyAttackCache();
    }

    public override void OnLeftRoom() => spawned = false;

    public override void OnDisconnected(DisconnectCause cause) => spawned = false;

    // 플레이어 퇴장 시 캔버스 제거 및 EnemyAttack 캐시 갱신
    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        int actorNumber = otherPlayer.ActorNumber;

        if (canvasByActorNumber.TryGetValue(actorNumber, out GameObject canvasObj))
        {
            if (canvasObj != null)
                Destroy(canvasObj);

            canvasByActorNumber.Remove(actorNumber);
        }

        RefreshEnemyAttackCache();
    }

    private void OnDestroy()
    {
        foreach (var pair in canvasByActorNumber)
        {
            if (pair.Value != null)
                Destroy(pair.Value);
        }

        canvasByActorNumber.Clear();
    }

    // 현재 방의 다른 플레이어 중 동일 닉네임 존재 여부 확인
    private bool HasDuplicateNickname()
    {
        string myNickname = PhotonNetwork.NickName;

        foreach (Player other in PhotonNetwork.PlayerListOthers)
        {
            if (other.NickName == myNickname)
                return true;
        }

        return false;
    }

    // 플레이어 프리팹을 네트워크 인스턴스로 생성하고 초기 설정
    private void SpawnPlayer()
    {
        if (spawned) return;

        spawned = true;

        Vector3 spawnPos = spawnPoint != null ? spawnPoint.position : defaultSpawnPosition;
        Quaternion spawnRot = spawnPoint != null ? spawnPoint.rotation : Quaternion.Euler(defaultSpawnEuler);

        GameObject player = PhotonNetwork.Instantiate(playerPrefabName, spawnPos, spawnRot);

        PlayerActionLock actionLock = player.GetComponent<PlayerActionLock>();
        if (actionLock != null)
        {
            UIManager.Instance?.RegisterPlayer(actionLock);
            ChatManager.Instance?.RegisterPlayer(actionLock);
        }

        AttachCanvas(player, PhotonNetwork.NickName);
        StartCoroutine(RequestLoadAfterSpawnRoutine(player));

        Debug.Log($"플레이어 스폰 완료: {PhotonNetwork.NickName}");
    }

    // PlayerManager와 Stat이 준비된 뒤 세이브 데이터 로드
    private IEnumerator RequestLoadAfterSpawnRoutine(GameObject player)
    {
        PhotonView pv = player.GetComponent<PhotonView>();
        if (pv == null || !pv.IsMine)
            yield break;

        yield return new WaitUntil(() =>
            PlayerManager.Instance != null &&
            PlayerManager.Instance.Stat != null);

        // 두 프레임 대기해 컴포넌트 초기화 완료 보장
        yield return null;
        yield return null;

        SaveManager.Instance?.LoadPlayer();
    }

    // 1초 대기 후 신규 플레이어 오브젝트를 찾아 캔버스 부착
    private IEnumerator AttachCanvasToNewPlayerRoutine(Player newPlayer)
    {
        yield return new WaitForSeconds(1f);

        foreach (PhotonView pv in FindObjectsOfType<PhotonView>())
        {
            if (pv.Owner != newPlayer) continue;
            if (pv.GetComponent<PlayerManager>() == null) continue;

            AttachCanvas(pv.gameObject, newPlayer.NickName);
            yield break;
        }
    }

    // 이미 방에 있는 다른 플레이어들에게 캔버스 부착 (내 캐릭터 제외)
    private IEnumerator AttachCanvasToExistingPlayersRoutine()
    {
        yield return new WaitForSeconds(1f);

        foreach (PhotonView pv in FindObjectsOfType<PhotonView>())
        {
            if (pv.IsMine) continue;
            if (pv.GetComponent<PlayerManager>() == null) continue;

            AttachCanvas(pv.gameObject, pv.Owner.NickName);
        }
    }

    // 플레이어 오브젝트에 캔버스(이름표 + HP바)를 생성하고 연결
    private void AttachCanvas(GameObject player, string nickName)
    {
        PhotonView pv = player.GetComponent<PhotonView>();
        if (pv == null) return;

        int actorNumber = pv.OwnerActorNr;
        if (canvasByActorNumber.ContainsKey(actorNumber)) return;

        GameObject canvasPrefab = playerCanvasPrefab != null
            ? playerCanvasPrefab
            : Resources.Load<GameObject>("PlayerCanvas");

        if (canvasPrefab == null)
        {
            Debug.LogWarning("PlayerCanvas 프리팹을 찾을 수 없습니다.");
            return;
        }

        GameObject canvasObj = Instantiate(canvasPrefab);
        canvasByActorNumber.Add(actorNumber, canvasObj);

        PlayerHeadUI headUI = canvasObj.GetComponentInChildren<PlayerHeadUI>();
        if (headUI != null)
        {
            headUI.SetFollowTarget(player.transform);
            headUI.SetNickname(nickName);
        }

        PlayerHealthBar healthBar = canvasObj.GetComponentInChildren<PlayerHealthBar>();
        PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();

        if (healthBar != null && playerHealth != null)
            playerHealth.SetHealthBar(healthBar);
    }

    // 플레이어 입장/퇴장 시 모든 EnemyAttack의 캐시 갱신
    private void RefreshEnemyAttackCache()
    {
        foreach (EnemyAttack enemyAttack in FindObjectsOfType<EnemyAttack>())
            enemyAttack.RefreshCachedPlayers();
    }
}