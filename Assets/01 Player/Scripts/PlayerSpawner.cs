using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.SceneManagement;

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

    private static bool spawned = false;

    private readonly Dictionary<int, GameObject> canvasByActorNumber = new();

    private void Start()
    {
        TryEnterRoomOrConnect();
    }

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
        RoomOptions options = new RoomOptions
        {
            MaxPlayers = MaxPlayers
        };

        PhotonNetwork.JoinOrCreateRoom(RoomName, options, null);
    }

    public override void OnConnectedToMaster()
    {
        if (!spawned)
            JoinMainRoom();
    }

    public override void OnJoinedRoom()
    {
        if (spawned)
            return;

        if (HasDuplicateNickname())
        {
            Debug.LogWarning("중복 로그인 감지! 접속 종료");
            PhotonNetwork.LeaveRoom();
            SceneManager.LoadScene("Intro");
            return;
        }

        Debug.Log($"방 입장 완료 → 방 이름: {PhotonNetwork.CurrentRoom.Name}, 현재 인원: {PhotonNetwork.CurrentRoom.PlayerCount}");

        SpawnPlayer();
        StartCoroutine(AttachCanvasToExistingPlayersRoutine());
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        Debug.LogError($"방 입장 실패: {message}");
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        StartCoroutine(AttachCanvasToNewPlayerRoutine(newPlayer));
    }

    public override void OnLeftRoom()
    {
        spawned = false;
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        spawned = false;
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        int actorNumber = otherPlayer.ActorNumber;

        if (canvasByActorNumber.TryGetValue(actorNumber, out GameObject canvasObj))
        {
            if (canvasObj != null)
                Destroy(canvasObj);

            canvasByActorNumber.Remove(actorNumber);
        }
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

    private bool HasDuplicateNickname()
    {
        string myNickname = PhotonNetwork.NickName;

        foreach (Player otherPlayer in PhotonNetwork.PlayerListOthers)
        {
            if (otherPlayer.NickName == myNickname)
                return true;
        }

        return false;
    }

    private void SpawnPlayer()
    {
        if (spawned)
            return;

        spawned = true;

        Vector3 spawnPos = spawnPoint != null ? spawnPoint.position : defaultSpawnPosition;
        Quaternion spawnRot = spawnPoint != null
            ? spawnPoint.rotation
            : Quaternion.Euler(defaultSpawnEuler);

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

    private IEnumerator RequestLoadAfterSpawnRoutine(GameObject player)
    {
        PhotonView photonView = player.GetComponent<PhotonView>();
        if (photonView == null || !photonView.IsMine)
            yield break;

        yield return new WaitUntil(() =>
            PlayerManager.Instance != null &&
            PlayerManager.Instance.Stat != null);

        yield return null;
        yield return null;

        SaveManager.Instance?.LoadPlayer();
    }

    private IEnumerator AttachCanvasToNewPlayerRoutine(Player newPlayer)
    {
        yield return new WaitForSeconds(1f);

        foreach (PhotonView photonView in FindObjectsOfType<PhotonView>())
        {
            if (photonView.Owner != newPlayer)
                continue;

            if (photonView.GetComponent<PlayerManager>() == null)
                continue;

            AttachCanvas(photonView.gameObject, newPlayer.NickName);
            yield break;
        }
    }

    private IEnumerator AttachCanvasToExistingPlayersRoutine()
    {
        yield return new WaitForSeconds(1f);

        foreach (PhotonView photonView in FindObjectsOfType<PhotonView>())
        {
            if (photonView.IsMine)
                continue;

            if (photonView.GetComponent<PlayerManager>() == null)
                continue;

            AttachCanvas(photonView.gameObject, photonView.Owner.NickName);
        }
    }

    private void AttachCanvas(GameObject player, string nickName)
    {
        PhotonView photonView = player.GetComponent<PhotonView>();
        if (photonView == null)
            return;

        int actorNumber = photonView.OwnerActorNr;

        if (canvasByActorNumber.ContainsKey(actorNumber))
            return;

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
}