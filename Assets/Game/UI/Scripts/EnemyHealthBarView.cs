using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EnemyHealthBarView : MonoBehaviour
{
    [SerializeField] private Slider healthSlider;
    [SerializeField] private TMP_Text healthText;

    private float remainingDisplayTime;

    public void SetHealth(float current, float maximum)
    {
        maximum = Mathf.Max(1f, maximum);

        if (healthSlider != null)
        {
            healthSlider.minValue = 0f;
            healthSlider.maxValue = maximum;
            healthSlider.value = Mathf.Clamp(current, 0f, maximum);
        }

        if (healthText != null)
        {
            healthText.text =
                $"{Mathf.CeilToInt(current)} / {Mathf.CeilToInt(maximum)}";
        }
    }

    public void ShowFor(float duration)
    {
        remainingDisplayTime = duration;
        gameObject.SetActive(true);
    }

    private void Update()
    {
        remainingDisplayTime -= Time.deltaTime;

        if (remainingDisplayTime <= 0f)
            gameObject.SetActive(false);
    }
}