using UnityEngine;
using UnityEngine.UI;

public class PlayerHealthBar : MonoBehaviour
{
    [SerializeField] private Image fillImage;

    public void UpdateHealthBar(float currentHp, float maxHp)
    {
        if (maxHp <= 0f)
            return;

        fillImage.fillAmount = currentHp / maxHp;
    }
}