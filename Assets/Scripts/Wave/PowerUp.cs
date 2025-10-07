using UnityEngine;

[CreateAssetMenu(menuName = "PowerUp")]
public class PowerUp : ScriptableObject
{
    public string powerUpName;
    public string description;
    public Sprite icon;

    [Header("Tier")]
    public PowerUpTier tier = PowerUpTier.Minor;

    public virtual void Apply(PlayerInputHandler player)
    {
        // Implement in derived classes
    }
}