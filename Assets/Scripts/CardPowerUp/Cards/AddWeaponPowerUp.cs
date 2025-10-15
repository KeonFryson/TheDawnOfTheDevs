using UnityEngine;

[CreateAssetMenu(menuName = "PowerUp/AddWeapon")]
public class AddWeaponPowerUp : PowerUp
{
    [Header("Weapon To Add")]
    public PlayerWeaponHandler.WeaponType weaponType;

    public GameObject weaponPrefab;

    [Header("Weapon Stats")]
    public WeaponStats weaponStats;

    public override void Apply(PlayerInputHandler player)
    {
        var handler = player.GetComponent<PlayerWeaponHandler>();
        if (handler == null) return;


        Debug.Log($"[AddWeaponPowerUp] weaponStats: {(weaponStats != null ? weaponStats.ToString() : "null")}");
        Debug.Log($"[AddWeaponPowerUp] weaponStats.maxClipAmmo: {(weaponStats != null ? weaponStats.maxClipAmmo : -1)}");
        Debug.Log($"[AddWeaponPowerUp] weaponStats.maxAmmo: {(weaponStats != null ? weaponStats.maxAmmo : -1)}");


        int clipAmmo = weaponStats != null ? weaponStats.maxClipAmmo : 100;
        int maxClipAmmo = weaponStats != null ? weaponStats.maxClipAmmo : 100;
        int reserveAmmo = weaponStats != null ? weaponStats.maxAmmo : 300;
        int maxReserveAmmo = reserveAmmo;

        Debug.Log($"[AddWeaponPowerUp] clipAmmo: {clipAmmo}, maxClipAmmo: {maxClipAmmo}, reserveAmmo: {reserveAmmo}, maxReserveAmmo: {maxReserveAmmo}");

        // Try to add weapon, returns true if added, false if already 2 weapons
        if (!handler.AddWeaponToSlots(
                weaponType,
                weaponPrefab,
                clipAmmo,
                maxClipAmmo,
                reserveAmmo,
                maxReserveAmmo,
                weaponStats))
        {
            // Already have 2 weapons, prompt to remove one
            var ui = FindFirstObjectByType<PowerUpUI>();
            if (ui != null)
            {
                Debug.Log($"[AddWeaponPowerUp] Prompting weapon replace dialog for: {weaponType}");
                ui.ShowWeaponReplaceDialog(handler, weaponType, weaponPrefab, weaponStats);
            }
            else
            {
                Debug.LogWarning("[AddWeaponPowerUp] PowerUpUI not found in scene.");
            }
        }
        else
        {
            Debug.Log($"[AddWeaponPowerUp] Added weapon: {weaponType}");
        }
    }
}