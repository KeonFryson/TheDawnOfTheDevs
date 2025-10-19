using UnityEngine;


[CreateAssetMenu(menuName = "PowerUp/AmmoBox")]
public class AmmoBox : PowerUp
{
    public enum TargetSlotMode { All, Current, Specific }

    [Header("Ammo Increase (%)")]
    [Range(0, 100)]
    public int ammoIncreasePercent = 25;

    [Header("Target")]
    public TargetSlotMode targetMode = TargetSlotMode.Current;
    [Tooltip("Used when TargetSlotMode == Specific")]
    public int targetSlotIndex = 0;

    public override void Apply(PlayerInputHandler player)
    {
        // Try to find a PlayerWeaponHandler on the player first, then in scene
        PlayerWeaponHandler handler = null;
        if (player != null)
            handler = player.GetComponent<PlayerWeaponHandler>();

        if (handler == null)
            handler = FindFirstObjectByType<PlayerWeaponHandler>();

        if (handler == null)
        {
            Debug.LogWarning("[AmmoBox] No PlayerWeaponHandler found in player or scene. Ammo not applied.");
            return;
        }

        if (ammoIncreasePercent <= 0)
        {
            Debug.LogWarning("[AmmoBox] ammoIncreasePercent is zero or negative. Nothing to apply.");
            return;
        }

        switch (targetMode)
        {
            case TargetSlotMode.All:
                for (int i = 0; i < handler.weaponSlots.Count; i++)
                {
                    handler.IncreaseMaxReserveForSlot(i, ammoIncreasePercent);
                }
                break;

            case TargetSlotMode.Current:
                {
                    int currentIdx = handler.GetCurrentWeaponSlot();
                    handler.IncreaseMaxReserveForSlot(currentIdx, ammoIncreasePercent);
                    break;
                }

            case TargetSlotMode.Specific:
                {
                    // If we have a player and the handler belongs to the player, show UI cards for slot selection.
                    if (player != null && handler == player.GetComponent<PlayerWeaponHandler>())
                    {
                        var ui = FindFirstObjectByType<PowerUpUI>();
                        if (ui != null)
                        {
                            ui.ShowAmmoSlotSelection(handler, ammoIncreasePercent);
                            return;
                        }
                        // If no UI, fall back to keyboard/coroutine selection if available
                        handler.PromptPlayerChooseSlotForAmmo(ammoIncreasePercent);
                        return;
                    }
                    else
                    {
                        // Fallback: use configured targetSlotIndex (clamped)
                        int clamped = Mathf.Clamp(targetSlotIndex, 0, handler.weaponSlots.Count - 1);
                        handler.IncreaseMaxReserveForSlot(clamped, ammoIncreasePercent);
                    }
                    break;
                }
        }
    }
}