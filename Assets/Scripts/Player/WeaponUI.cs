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

    public void UpdateWeaponDisplay(PlayerWeaponHandler.WeaponSlot slot)
    {
        var handler = FindFirstObjectByType<PlayerWeaponHandler>();

        // Main weapon display
        if (slot.ammo <= 0)
        {
            weaponImage.sprite = GetWeaponSprite(slot.type);
            currentAmmoText.text = "0";
        }
        else
        {
            weaponImage.sprite = GetWeaponSprite(slot.type);
            currentAmmoText.text = slot.ammo.ToString();
        }
        maxAmmoText.text = slot.maxAmmo.ToString();

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
}