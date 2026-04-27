using UnityEngine;
using UnityEngine.UI;

// UI Image의 fillAmount로 HP 비율 표시
public class PlayerHealthBar : MonoBehaviour
{
    [SerializeField] private Image fillImage;

    public void UpdateHealthBar(float currentHp, float maxHp)
    {
        if (fillImage == null || maxHp <= 0f) return;
        fillImage.fillAmount = Mathf.Clamp01(currentHp / maxHp);
    }
}