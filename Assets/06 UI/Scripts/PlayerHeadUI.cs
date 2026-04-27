using TMPro;
using UnityEngine;

// 플레이어 머리 위 레벨 + 이름 표시 및 위치 추적
public class PlayerHeadUI : MonoBehaviour
{
    [SerializeField] private TMP_Text levelAndNameText;
    [SerializeField] private Vector3 offset = new Vector3(0, 2f, 0);

    private Transform followTarget;
    private string nickname = "Player";

    public void SetFollowTarget(Transform target)
    {
        followTarget = target;
    }

    public void SetNickname(string name)
    {
        nickname = name;
        UpdateUI(PlayerManager.Instance?.Stat?.Level ?? 1);
    }

    private void Start()
    {
        // 레벨 변경 이벤트 등록
        if (PlayerManager.Instance != null && PlayerManager.Instance.Stat != null)
            PlayerManager.Instance.Stat.OnLevelChanged += UpdateUI;

        UpdateUI(PlayerManager.Instance?.Stat?.Level ?? 1);
    }

    private void OnDestroy()
    {
        // 이벤트 해제
        if (PlayerManager.Instance != null && PlayerManager.Instance.Stat != null)
            PlayerManager.Instance.Stat.OnLevelChanged -= UpdateUI;
    }

    private void Update()
    {
        // 대상 사라지면 UI 제거
        if (followTarget == null)
            Destroy(gameObject);
    }

    private void LateUpdate()
    {
        if (followTarget == null) return;

        // 위치 추적
        transform.position = followTarget.position + offset;

        // 카메라 바라보게
        Camera cam = Camera.main;
        if (cam == null) return;

        transform.rotation = Quaternion.LookRotation(
            transform.position - cam.transform.position
        );
    }

    // UI 텍스트 갱신
    private void UpdateUI(int level)
    {
        levelAndNameText?.SetText($"Lv. {level} {nickname}");
    }
}