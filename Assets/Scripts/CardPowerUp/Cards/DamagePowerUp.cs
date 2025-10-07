using UnityEngine;

[CreateAssetMenu(menuName = "PowerUp/Damage")]
public class DamagePowerUp : PowerUp
{
    [Header("Damage Increase")]
    public int damageIncrease = 5;

    public override void Apply(PlayerInputHandler player)
    {
        // Assuming PlayerInputHandler has a method or property to increase damage.
        // If not, you need to add one.
        player.IncreaseDamage(damageIncrease);
    }
}