using UnityEngine;

[CreateAssetMenu(menuName = "PowerUp/Max Health Boost")]
public class MaxHealthBoost : PowerUp
{
    [Header("Max Health Increase")]
    public float maxHealthIncrease = 25f;

    public override void Apply(PlayerInputHandler player)
    {
       player.ChangeMaxHealth(maxHealthIncrease);
       player.ChangeHealth(maxHealthIncrease); // Heal the player by the same amount
    }
}