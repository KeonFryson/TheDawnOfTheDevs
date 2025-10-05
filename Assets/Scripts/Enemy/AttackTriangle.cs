using UnityEngine;

public class AttackTriangle : MonoBehaviour
{
    private Blue_Enemy enemy;

    private void Awake()
    {
        enemy = GetComponentInParent<Blue_Enemy>();
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (enemy == null) return;
        if (other.TryGetComponent<PlayerInputHandler>(out var player))
        {
            enemy.TryAttackPlayer(player);
        }
    }
}