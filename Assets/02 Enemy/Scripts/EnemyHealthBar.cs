using UnityEngine;
using UnityEngine.UI;

public class EnemyHealthBar : MonoBehaviour
{
    [SerializeField] private Image fillImage;
    [SerializeField] private float targetSearchInterval = 0.5f;
    [SerializeField] private float visibleCheckInterval = 0.2f;

    private EnemyHealth enemyHealth;
    private Transform player;
    private Canvas canvas;

    private float targetSearchTimer;
    private float visibleCheckTimer;

    private void Awake()
    {
        enemyHealth = GetComponentInParent<EnemyHealth>();
        canvas = GetComponent<Canvas>();

        if (canvas == null)
            canvas = GetComponentInChildren<Canvas>(true);
    }

    private void Start()
    {
        SetVisible(false);
        FindNearestPlayer();
    }

    private void Update()
    {
        if (enemyHealth == null || enemyHealth.EnemyData == null)
            return;

        targetSearchTimer -= Time.deltaTime;
        if (targetSearchTimer <= 0f)
        {
            targetSearchTimer = targetSearchInterval;
            FindNearestPlayer();
        }

        visibleCheckTimer -= Time.deltaTime;
        if (visibleCheckTimer > 0f)
            return;

        visibleCheckTimer = visibleCheckInterval;

        bool shouldShow = ShouldShowHealthBar();
        SetVisible(shouldShow);
    }

    private void FindNearestPlayer()
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");

        float minDist = float.MaxValue;
        Transform nearest = null;

        foreach (GameObject p in players)
        {
            if (p == null || !p.activeInHierarchy)
                continue;

            float dist = Vector3.Distance(enemyHealth.transform.position, p.transform.position);
            if (dist < minDist)
            {
                minDist = dist;
                nearest = p.transform;
            }
        }

        player = nearest;
    }

    private bool ShouldShowHealthBar()
    {
        if (player == null)
            return false;

        if (enemyHealth.IsDead)
            return false;

        float distance = Vector3.Distance(player.position, enemyHealth.transform.position);
        return distance <= enemyHealth.EnemyData.detectRange;
    }

    public void UpdateHealthBar(float currentHp, float maxHp)
    {
        if (fillImage == null || maxHp <= 0f)
            return;

        fillImage.fillAmount = Mathf.Clamp01(currentHp / maxHp);
    }

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