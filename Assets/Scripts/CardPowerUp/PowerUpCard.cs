using UnityEngine;
using UnityEngine.UI;
using TMPro;

public enum PowerUpTier
{
    Minor = 1,
    Major = 2,
    Ultimate = 3
}
public class PowerUpCard : MonoBehaviour
{
    public Image icon;
    public TMP_Text nameText;
    public TMP_Text descriptionText;
    public TMP_Text tierTextTop;
    public TMP_Text tierTexTBottom;

    private PowerUp powerUp;
    private System.Action<PowerUp> onSelected;

    // Standard setup for power-up selection
    public void Setup(PowerUp powerUp, System.Action<PowerUp> onSelected)
    {
        this.powerUp = powerUp;
        this.onSelected = onSelected;

        if (icon != null) icon.sprite = powerUp.icon;
        if (nameText != null) nameText.text = powerUp.powerUpName;
        if (descriptionText != null) descriptionText.text = powerUp.description;
        if (tierTextTop != null) tierTextTop.text = powerUp.tier.ToString();
        if (tierTexTBottom != null) tierTexTBottom.text = powerUp.tier.ToString();
    }

    // Setup for weapon replacement dialog
   public void SetupForWeaponReplace(PlayerWeaponHandler.WeaponType weaponType, System.Action onSelected)
    {
        if (icon != null) icon.sprite = null; // Optionally set a weapon icon
        if (nameText != null) nameText.text = weaponType.ToString();
        if (descriptionText != null) descriptionText.text = "Click to replace this weapon with the new one!";
        if (tierTextTop != null) tierTextTop.text = "";
        if (tierTexTBottom != null) tierTexTBottom.text = "";

        this.powerUp = null;
        this.onSelected = _ => onSelected?.Invoke();

        Debug.Log($"[PowerUpCard] SetupForWeaponReplace called for weaponType: {weaponType}");
    }


    // Called by UI button
    public void OnClick()
    {
        var ui = FindFirstObjectByType<PowerUpUI>();
        if (ui != null && !ui.CanSelectCard())
            return; // Block input if not allowed

        if (powerUp != null)
            onSelected?.Invoke(powerUp);
        else
            onSelected?.Invoke(null);
    }
}