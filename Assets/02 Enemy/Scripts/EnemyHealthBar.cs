using UnityEngine;
using UnityEngine.UI;

// 적 HP바 UI - 가장 가까운 플레이어가 탐지 범위 내에 있을 때만 표시
public class EnemyHealthBar : MonoBehaviour
{
    [SerializeField] private Image fillImage;
    [SerializeField] private float targetSearchInterval = 0.5f; // 가장 가까운 플레이어 재탐색 주기
    [SerializeField] private float visibleCheckInterval = 0.2f; // HP바 표시 여부 갱신 주기

    private EnemyHealth enemyHealth;
    private Transform player;
    private Canvas canvas;

    private float targetSearchTimer;
    private float visibleCheckTimer;

    private void Awake()
    {
        enemyHealth = GetComponentInParent<EnemyHealth>();
        canvas = GetComponent<Canvas>() ?? GetComponentInChildren<Canvas>(true);
    }

    private void Start()
    {
        SetVisible(false);
        FindNearestPlayer();
    }

    private void Update()
    {
        if (enemyHealth == null || enemyHealth.EnemyData == null) return;

        // 주기적으로 가장 가까운 플레이어 갱신
        targetSearchTimer -= Time.deltaTime;
        if (targetSearchTimer <= 0f)
        {
            targetSearchTimer = targetSearchInterval;
            FindNearestPlayer();
        }

        // 주기적으로 HP바 표시 여부 갱신
        visibleCheckTimer -= Time.deltaTime;
        if (visibleCheckTimer > 0f) return;

        visibleCheckTimer = visibleCheckInterval;
        SetVisible(ShouldShowHealthBar());
    }

    // 활성 상태인 플레이어 중 가장 가까운 대상을 탐색
    private void FindNearestPlayer()
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        float minDist = float.MaxValue;
        Transform nearest = null;

        foreach (GameObject p in players)
        {
            if (p == null || !p.activeInHierarchy) continue;

            float dist = Vector3.Distance(enemyHealth.transform.position, p.transform.position);
            if (dist < minDist)
            {
                minDist = dist;
                nearest = p.transform;
            }
        }

        player = nearest;
    }

    // 플레이어가 탐지 범위 내에 있고 적이 살아 있을 때만 true
    private bool ShouldShowHealthBar()
    {
        if (player == null || enemyHealth.IsDead) return false;

        float distance = Vector3.Distance(player.position, enemyHealth.transform.position);
        return distance <= enemyHealth.EnemyData.detectRange;
    }

    public void UpdateHealthBar(float currentHp, float maxHp)
    {
        if (fillImage == null || maxHp <= 0f) return;
        fillImage.fillAmount = Mathf.Clamp01(currentHp / maxHp);
    }

    // Canvas가 있으면 enabled로 제어, 없으면 하위 Renderer 전체 제어
    private void SetVisible(bool visible)
    {
        if (canvas != null)
        {
            if (canvas.enabled != visible)
                canvas.enabled = visible;
            return;
        }

        Renderer[] renderers = GetComponentsInChildren<Renderer>(true);
        for (int i = 0; i < renderers.Length; i++)
            renderers[i].enabled = visible;
    }
}