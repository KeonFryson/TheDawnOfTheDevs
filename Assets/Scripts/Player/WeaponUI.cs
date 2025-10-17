using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WeaponUI : MonoBehaviour
{
    [Header("Weapon UI Elements")]
    public Image weaponImage;
    public TMP_Text currentAmmoText;
    public TMP_Text maxAmmoText;

    [Header("Second Weapon UI Elements")]
    public Image secondWeaponImage;

    [Header("Weapon Sprites")]
    public Sprite pistolSprite;
    public Sprite shotgunSprite;
    public Sprite laserSprite;

    [Header("Grenade UI")]
    public Image grenadeIcon1;
    public TMP_Text grenadeText1;

    public Image grenadeIcon2;
    public TMP_Text grenadeText2;
    public TMP_Text ControlGrenade;



    /// <summary>
    /// Update main weapon slot display.
    /// </summary>
    public void UpdateWeaponDisplay(PlayerWeaponHandler.WeaponSlot slot)
    {
        var handler = FindFirstObjectByType<PlayerWeaponHandler>();

        // Main weapon display
        if (weaponImage != null) weaponImage.sprite = GetWeaponSprite(slot.type);
        if (currentAmmoText != null) currentAmmoText.text = slot.clipAmmo.ToString();      // Show current clip ammo
        if (maxAmmoText != null) maxAmmoText.text = slot.reserveAmmo.ToString();       // Show reserve ammo

        // Second weapon display: always show the other weapon if it exists
        if (secondWeaponImage != null && handler != null && handler.weaponSlots.Count > 1)
        {
            int currentSlot = handler.weaponSlots.IndexOf(slot);
            int secondSlotIndex = (currentSlot == 0) ? 1 : 0;
            var secondSlot = handler.weaponSlots[secondSlotIndex];

            secondWeaponImage.enabled = true;
            secondWeaponImage.sprite = GetWeaponSprite(secondSlot.type);
        }
        else if (secondWeaponImage != null)
        {
            secondWeaponImage.enabled = false;
            secondWeaponImage.sprite = null;
        }
    }

    private Sprite GetWeaponSprite(PlayerWeaponHandler.WeaponType type)
    {
        switch (type)
        {
            case PlayerWeaponHandler.WeaponType.Pistol: return pistolSprite;
            case PlayerWeaponHandler.WeaponType.Shotgun: return shotgunSprite;
            case PlayerWeaponHandler.WeaponType.Laser: return laserSprite;
            default: return null;
        }
    }

    /// <summary>
    /// Update grenade UI. Called by PlayerGrenadeHandler.UpdateUI().
    /// visibleSelection: 0 = none, 1 = highlight slot1, 2 = highlight slot2
    /// 
    /// There is no separate select image; icons and text will use alpha to indicate selection:
    /// selected = alpha 1.0, not-selected (but available) = alpha 0.5.
    /// </summary>
    public void UpdateGrenadeDisplay(Sprite icon1, int count1, Sprite icon2, int count2, int visibleSelection = 0)
    {
        // Slot 1
        if (grenadeIcon1 != null)
        {
            grenadeIcon1.sprite = icon1;
            grenadeIcon1.enabled = (count1 > 0 && icon1 != null);
        }
        if (grenadeText1 != null)
        {
            grenadeText1.text = count1 > 0 ? count1.ToString() : "";
            grenadeText1.enabled = (count1 > 0);
        }

        if (count1 > 0 && icon1 != null)
        {
            float alpha = (visibleSelection == 1) ? 1f : 0.5f;
            if (grenadeIcon1 != null) SetImageAlpha(grenadeIcon1, alpha);
            if (grenadeText1 != null) SetTextAlpha(grenadeText1, alpha);
            if (ControlGrenade != null) SetTextAlpha(ControlGrenade, alpha);
        }

        // Slot 2
        if (grenadeIcon2 != null)
        {
            grenadeIcon2.sprite = icon2;
            grenadeIcon2.enabled = (count2 > 0 && icon2 != null);
        }
        if (grenadeText2 != null)
        {
            grenadeText2.text = count2 > 0 ? count2.ToString() : "";
            grenadeText2.enabled = (count2 > 0);
        }

        if (count2 > 0 && icon2 != null)
        {
            float alpha = (visibleSelection == 2) ? 1f : 0.5f;
            if (grenadeIcon2 != null) SetImageAlpha(grenadeIcon2, alpha);
            if (grenadeText2 != null) SetTextAlpha(grenadeText2, alpha);
            if (ControlGrenade != null) SetTextAlpha(ControlGrenade, alpha);
        }
        // Control text visibility: only show if at least one slot has grenades
        bool anyGrenade = (count1 > 0) || (count2 > 0);
        if (ControlGrenade != null)
        {
            ControlGrenade.enabled = anyGrenade;

            if (anyGrenade)
            {
                // Determine alpha for control text based on selection:
                float controlAlpha = 0.5f; // default when any grenade exists but not explicitly selected
                if (visibleSelection == 1 && count1 > 0) controlAlpha = 1f;
                else if (visibleSelection == 2 && count2 > 0) controlAlpha = 1f;

                SetTextAlpha(ControlGrenade, controlAlpha);
            }
            else
            {
                // Ensure fully transparent if disabled (defensive)
                SetTextAlpha(ControlGrenade, 0f);
            }
        }
    }

    private void SetImageAlpha(Image img, float alpha)
    {
        if (img == null) return;
        var c = img.color;
        c.a = Mathf.Clamp01(alpha);
        img.color = c;
    }

    private void SetTextAlpha(TMP_Text txt, float alpha)
    {
        if (txt == null) return;
        var c = txt.color;
        c.a = Mathf.Clamp01(alpha);
        txt.color = c;
    }
}