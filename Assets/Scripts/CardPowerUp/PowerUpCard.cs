using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems; // Add this for pointer events

public enum PowerUpTier
{
    Minor = 1,
    Major = 2,
    Ultimate = 3
}
public class PowerUpCard : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public Image icon;
    public TMP_Text nameText;
    public TMP_Text descriptionText;
    public TMP_Text tierTextTop;
    public TMP_Text tierTexTBottom;

    private PowerUp powerUp;
    private System.Action<PowerUp> onSelected;

    private Vector3 originalScale;
    private bool scaleInitialized = false;
    private float hoverScale = 1.1f;

    public Sprite pistolIcon;
    public Sprite shotgunIcon;
    public Sprite laserIcon;

    public Vector2 pistolIconScale = Vector2.one;
    public Vector2 shotgunIconScale = Vector2.one;
    public Vector2 laserIconScale = Vector2.one;

    private Sprite GetWeaponTypeIcon(PlayerWeaponHandler.WeaponType weaponType)
    {
        return weaponType switch
        {
            PlayerWeaponHandler.WeaponType.Pistol => pistolIcon,
            PlayerWeaponHandler.WeaponType.Shotgun => shotgunIcon,
            PlayerWeaponHandler.WeaponType.Laser => laserIcon,
            _ => null
        };
    }
    private Vector2 GetWeaponTypeIconScale(PlayerWeaponHandler.WeaponType weaponType)
    {
        return weaponType switch
        {
            PlayerWeaponHandler.WeaponType.Pistol => pistolIconScale,
            PlayerWeaponHandler.WeaponType.Shotgun => shotgunIconScale,
            PlayerWeaponHandler.WeaponType.Laser => laserIconScale,
            _ => Vector2.one
        };
    }
    // Standard setup for power-up selection
    public void Setup(PowerUp powerUp, System.Action<PowerUp> onSelected)
    {
        this.powerUp = powerUp;
        this.onSelected = onSelected;

        if (icon != null) icon.sprite = powerUp.icon;
        icon.transform.localScale = new Vector3(powerUp.iconScale.x, powerUp.iconScale.y, 1f);
        if (nameText != null) nameText.text = powerUp.powerUpName;
        if (descriptionText != null) descriptionText.text = powerUp.description;
        if (tierTextTop != null) tierTextTop.text = powerUp.tier.ToString();
        if (tierTexTBottom != null) tierTexTBottom.text = powerUp.tier.ToString();
    }

    // Setup for weapon replacement dialog
    public void SetupForWeaponReplace(PlayerWeaponHandler.WeaponType weaponType, System.Action onSelected)
    {
        if (icon != null)
        {
            icon.sprite = GetWeaponTypeIcon(weaponType);
            icon.transform.localScale = new Vector3(GetWeaponTypeIconScale(weaponType).x, GetWeaponTypeIconScale(weaponType).y, 1f);
        }

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

    // Hover effect handlers
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!scaleInitialized)
        {
            originalScale = transform.localScale;
            scaleInitialized = true;
        }
        transform.localScale = originalScale * hoverScale;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (scaleInitialized)
            transform.localScale = originalScale;
    }
}