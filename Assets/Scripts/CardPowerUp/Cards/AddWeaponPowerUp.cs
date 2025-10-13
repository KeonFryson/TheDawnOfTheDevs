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

        // Try to add weapon, returns true if added, false if already 2 weapons
        if (!handler.AddWeaponToSlots(
                weaponType,
                weaponPrefab,
                weaponStats != null ? weaponStats.maxAmmo : 100,
                weaponStats != null ? weaponStats.maxAmmo : 100,
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