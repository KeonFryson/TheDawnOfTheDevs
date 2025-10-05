// ============================
// PowerUpCard.cs
// ============================
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PowerUpCard : MonoBehaviour
{
    public Image icon;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI descriptionText;

    private PowerUp powerUp;
    private System.Action<PowerUp> onSelected;

    public void Setup(PowerUp powerUp, System.Action<PowerUp> onSelected)
    {
        this.powerUp = powerUp;
        this.onSelected = onSelected;

        if (icon != null) icon.sprite = powerUp.icon;
        if (nameText != null) nameText.text = powerUp.powerUpName;
        if (descriptionText != null) descriptionText.text = powerUp.description;
    }

    public void OnClick()
    {
        onSelected?.Invoke(powerUp);
    }
}
