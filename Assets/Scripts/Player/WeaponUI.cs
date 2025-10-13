using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WeaponUI : MonoBehaviour
{
    [Header("Weapon UI Elements")]
    public Image weaponImage;
    public TMP_Text currentAmmoText;
    public TMP_Text maxAmmoText;

    [Header("Weapon Sprites")]
    public Sprite pistolSprite;
    public Sprite shotgunSprite;
    public Sprite laserSprite;

    public void UpdateWeaponDisplay(PlayerWeaponHandler.WeaponSlot slot)
    {
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