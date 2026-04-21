using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using System.Collections;

public class PlayerSpawner : MonoBehaviourPunCallbacks
{
    [SerializeField] private string playerPrefabName = "Player";
    [SerializeField] private Transform spawnPoint;

    private static bool _spawned = false;

    private void Start()
    {
        _spawned = false;

        if (PhotonNetwork.InRoom)
            SpawnPlayer();
        else if (PhotonNetwork.IsConnectedAndReady)
            PhotonNetwork.JoinOrCreateRoom("MainRoom", new RoomOptions { MaxPlayers = 10 }, null);
        else
            PhotonNetwork.ConnectUsingSettings();
    }

    public override void OnConnectedToMaster()
    {
        if (!_spawned)
            PhotonNetwork.JoinOrCreateRoom("MainRoom", new RoomOptions { MaxPlayers = 10 }, null);
    }

    public override void OnJoinedRoom()
    {
        if (_spawned) return;

        // 같은 닉네임이 이미 방에 있으면 강퇴
        string myNickname = PhotonNetwork.NickName;
        foreach (var player in PhotonNetwork.PlayerListOthers)
        {
            if (player.NickName == myNickname)
            {
                Debug.LogWarning("중복 로그인 감지! 접속 종료");
                PhotonNetwork.LeaveRoom();
                // 로그인 씬으로 이동
                UnityEngine.SceneManagement.SceneManager.LoadScene("Intro");
                return;
            }
        }

        Debug.Log($"방 입장 완료 → 방 이름: {PhotonNetwork.CurrentRoom.Name}, 현재 인원: {PhotonNetwork.CurrentRoom.PlayerCount}");
        SpawnPlayer();
        StartCoroutine(AttachCanvasToExistingPlayers());
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        Debug.LogError($"방 입장 실패: {message}");
    }

    // 다른 플레이어가 새로 들어왔을 때
    public override void OnPlayerEnteredRoom(Photon.Realtime.Player newPlayer)
    {
        StartCoroutine(AttachCanvasToNewPlayer(newPlayer));
    }

    private IEnumerator AttachCanvasToNewPlayer(Photon.Realtime.Player player)
    {
        yield return new WaitForSeconds(1f);

        foreach (var pv in FindObjectsOfType<PhotonView>())
        {
            if (pv.Owner == player && pv.GetComponent<PlayerManager>() != null)
            {
                AttachCanvas(pv.gameObject, player.NickName);
                break;
            }
        }
    }

    private IEnumerator AttachCanvasToExistingPlayers()
    {
        yield return new WaitForSeconds(1f);

        foreach (var pv in FindObjectsOfType<PhotonView>())
        {
            if (pv.IsMine) continue;
            if (pv.GetComponent<PlayerManager>() == null) continue;

            AttachCanvas(pv.gameObject, pv.Owner.NickName);
        }
    }

    private void SpawnPlayer()
    {
        if (_spawned) return;
        _spawned = true;

        Vector3 spawnPos = spawnPoint != null ? spawnPoint.position : new Vector3(9.63f, 1.76f, 59.84f);
        Quaternion spawnRot = spawnPoint != null ? spawnPoint.rotation : Quaternion.identity;

        GameObject player = PhotonNetwork.Instantiate(playerPrefabName, spawnPos, spawnRot);
        UIManager.Instance?.RegisterPlayer(player.GetComponent<PlayerActionLock>());

        AttachCanvas(player, PhotonNetwork.NickName);

        Debug.Log($"플레이어 스폰 완료: {PhotonNetwork.NickName}");
    }

    private void AttachCanvas(GameObject player, string nickName)
    {
        GameObject canvasObj = Instantiate(Resources.Load<GameObject>("PlayerCanvas"));
        canvasObj.transform.SetParent(null);

        PlayerHeadUI headUI = canvasObj.GetComponentInChildren<PlayerHeadUI>();
        if (headUI != null)
        {
            headUI.SetFollowTarget(player.transform);
            headUI.SetNickname(nickName);
        }
    }
}