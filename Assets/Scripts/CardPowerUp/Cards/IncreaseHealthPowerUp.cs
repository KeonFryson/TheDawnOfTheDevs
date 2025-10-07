using UnityEngine;

[CreateAssetMenu(menuName = "PowerUp/IncreaseHealth")]
public class IncreaseHealthPowerUp : PowerUp
{
    [Header("Health Increase")]
    public int healthIncrease = 1;

    public override void Apply(PlayerInputHandler player)
    {
        player.ChangeHealth(healthIncrease);
    }
}