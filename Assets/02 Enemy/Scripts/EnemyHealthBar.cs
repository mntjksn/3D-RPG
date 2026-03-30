using UnityEngine;
using UnityEngine.UI;

public class EnemyHealthBar : MonoBehaviour
{
    [SerializeField] private Image fillImage;

    private EnemyHealth enemyHealth;
    private Transform player;

    private float checkTimer;

    private void Awake()
    {
        enemyHealth = GetComponentInParent<EnemyHealth>();

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
            player = playerObj.transform;
    }

    private void Start()
    {
        SetVisible(false); // 처음엔 숨김
    }

    private void Update()
    {
        if (enemyHealth == null || player == null)
            return;

        // 매 프레임 말고 0.2초마다 체크
        checkTimer -= Time.deltaTime;
        if (checkTimer > 0f)
            return;

        checkTimer = 0.2f;

        float distance = Vector3.Distance(player.position, enemyHealth.transform.position);

        bool shouldShow = distance <= enemyHealth.EnemyData.detectRange && !enemyHealth.IsDead;

        SetVisible(shouldShow);
    }

    public void UpdateHealthBar(float currentHp, float maxHp)
    {
        if (maxHp <= 0f)
            return;

        fillImage.fillAmount = currentHp / maxHp;
    }

    private void SetVisible(bool visible)
    {
        if (fillImage == null)
            return;

        // Canvas 기준으로 꺼주는게 더 안전
        Canvas canvas = GetComponent<Canvas>();
        if (canvas != null)
            canvas.enabled = visible;
        else
            gameObject.SetActive(visible);
    }
}