using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIHandler : MonoBehaviour
{
    public static UIHandler instance;

    [Header("Health UI")]
    [SerializeField] private Slider healthSlider;
    [SerializeField] private TMP_Text currentHpText;
    [SerializeField] private TMP_Text maxHpText;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
    }

    // Accepts a normalized value [0..1] and updates the slider.
    public void SetHealthValue(float normalized)
    {
        if (healthSlider == null) return;
        healthSlider.minValue = 0f;
        healthSlider.maxValue = 1f;
        healthSlider.value = Mathf.Clamp01(normalized);
    }

    // Accepts absolute values and updates slider + text fields.
    public void SetHealthAbsolute(float current, float max)
    {
        float norm = (max > 0f) ? Mathf.Clamp01(current / max) : 0f;
        SetHealthValue(norm);

        if (currentHpText != null)
            currentHpText.text = Mathf.RoundToInt(current).ToString();
        if (maxHpText != null)
            maxHpText.text = Mathf.RoundToInt(max).ToString();
    }
}