
using UnityEngine;

[CreateAssetMenu(menuName = "PowerUp/Grenade")]
public class GrenadePowerUp : PowerUp
{
    [Header("Grenade Amount to Give")]
    public int grenadeAmount = 3;

    [Header("Optional: give a specific grenade type")]
    public GrenadeData grenadeType;

    public override void Apply(PlayerInputHandler player)
    {
        // Try to find a PlayerGrenadeHandler on the player first, then in scene
        PlayerGrenadeHandler handler = null;
        if (player != null)
            handler = player.GetComponent<PlayerGrenadeHandler>();

        if (handler == null)
            handler = FindFirstObjectByType<PlayerGrenadeHandler>();

        if (handler == null)
        {
            Debug.LogWarning("[GrenadePowerUp] No PlayerGrenadeHandler found in player or scene. Grenades not applied.");
            return;
        }

        if (grenadeType != null)
        {
            handler.AddTypedGrenades(grenadeType, grenadeAmount);
            Debug.Log($"[GrenadePowerUp] Applied: gave {grenadeAmount}x {grenadeType.displayName}.");
        }
        else
        {
            handler.AddOrReplaceGrenades(grenadeAmount);
            Debug.Log($"[GrenadePowerUp] Applied: gave {grenadeAmount} untyped grenades.");
        }
    }
}