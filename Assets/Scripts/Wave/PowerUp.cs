using UnityEngine;

[CreateAssetMenu(menuName = "PowerUp")]
public class PowerUp : ScriptableObject
{
    public string powerUpName;
    public string description;
    public Sprite icon;

    [Header("Icon")]
    public Vector2 iconScale = Vector2.one;

    [Header("Tier")]
    public PowerUpTier tier = PowerUpTier.Minor;

    public virtual void Apply(PlayerInputHandler player)
    {
        // Implement in derived classes
    }
}