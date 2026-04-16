using TMPro;
using UnityEngine;

public class PlayerHeadUI : MonoBehaviour
{
    [SerializeField] private TMP_Text levelAndNameText;

    private PlayerStat playerStat;
    private string nickname = "Player";

    private void Start()
    {
        Bind();
        Subscribe();
        CacheNickname();
        RefreshNow();
    }

    private void OnDisable()
    {
        Unsubscribe();
    }

    private void Bind()
    {
        if (PlayerManager.Instance != null)
            playerStat = PlayerManager.Instance.Stat;
    }

    private void Subscribe()
    {
        if (playerStat == null)
            return;

        playerStat.OnLevelChanged += UpdateLevelAndNameUI;
    }

    private void Unsubscribe()
    {
        if (playerStat == null)
            return;

        playerStat.OnLevelChanged -= UpdateLevelAndNameUI;
    }

    private void CacheNickname()
    {
        if (FirebaseAuthManager.Instance != null)
        {
            string firebaseNickname = FirebaseAuthManager.Instance.GetNickname();

            if (!string.IsNullOrWhiteSpace(firebaseNickname))
                nickname = firebaseNickname;
        }
    }

    private void RefreshNow()
    {
        if (playerStat == null)
            return;

        UpdateLevelAndNameUI(playerStat.Level);
    }

    private void UpdateLevelAndNameUI(int level)
    {
        levelAndNameText.text = $"Lv. {level} {nickname}";
    }
}