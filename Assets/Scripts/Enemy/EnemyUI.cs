using UnityEngine;
using UnityEngine.UI;

public class EnemyUI : MonoBehaviour
{
    [SerializeField] Image healthBar;
    [SerializeField] RectTransform currentHealthBar;

    public void SetHealth(float currentHealth, float maxHealth)
    {
        float percentage = Mathf.Clamp01(currentHealth / maxHealth);

        currentHealthBar.localScale = new Vector3(
            percentage,
            1f,
            1f
        );
    }
}
