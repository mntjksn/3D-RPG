using TMPro;
using UnityEngine;

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
        if (PlayerManager.Instance != null && PlayerManager.Instance.Stat != null)
            PlayerManager.Instance.Stat.OnLevelChanged += UpdateUI;

        UpdateUI(PlayerManager.Instance?.Stat?.Level ?? 1);
    }

    private void Update()
    {
        // followTarget이 사라지면 (플레이어 나감) Canvas도 삭제
        if (followTarget == null)
        {
            Destroy(gameObject);
        }
    }

    private void LateUpdate()
    {
        if (followTarget != null)
            transform.position = followTarget.position + offset;

        if (Camera.main != null)
        {
            transform.rotation = Quaternion.LookRotation(
                transform.position - Camera.main.transform.position
            );
        }
    }

    private void UpdateUI(int level)
    {
        levelAndNameText.text = $"Lv. {level} {nickname}";
    }
}