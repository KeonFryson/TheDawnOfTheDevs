// ============================
// Blue_Enemy.cs
// ============================
using System;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Blue_Enemy : MonoBehaviour
{
    public float speed = 2f;
    public float health = 20f;

    public event Action OnDeath;

    private Rigidbody2D rb;
    private Vector2 moveDir = Vector2.right;
    private float changeDirTimer = 2f;
    private float timer;

    private bool isDead = false;

    public float attackRange = 1.5f;
    public float attackCooldown = 1f;
    private float attackTimer = 0f;

    public float separationRadius = 1.0f; // Minimum space between enemies
    public float separationStrength = 1.5f; // How strongly to push away

    [Header("Attack Area")]
    public Transform attackTriangle;

    [Header("Dodge Movement")]
    public float dodgeInterval = 2.5f; // How often to dodge (seconds)
    public float dodgeDuration = 0.4f; // How long the dodge lasts (seconds)
    public float dodgeStrength = 1.2f; // How strong the diagonal is

    private float dodgeTimer = 0f;
    private float dodgeTimeLeft = 0f;
    private int dodgeDirection = 0; // -1 = left, 1 = right, 0 = none

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        timer = changeDirTimer;
        dodgeTimer = UnityEngine.Random.Range(0, dodgeInterval); // Stagger dodges
    }

    private void FixedUpdate()
    {
        if (isDead) return;
        var player = FindFirstObjectByType<PlayerInputHandler>();
        if (player != null)
        {
            Vector2 dirToPlayer = ((Vector2)player.transform.position - rb.position).normalized;
            float distance = Vector2.Distance(rb.position, player.transform.position);

            // Handle dodge logic
            dodgeTimer -= Time.fixedDeltaTime;
            if (dodgeTimeLeft > 0f)
            {
                dodgeTimeLeft -= Time.fixedDeltaTime;
            }
            else if (dodgeTimer <= 0f)
            {
                dodgeTimeLeft = dodgeDuration;
                dodgeTimer = dodgeInterval + UnityEngine.Random.Range(-0.5f, 0.5f); // Add some randomness
                dodgeDirection = UnityEngine.Random.value < 0.5f ? -1 : 1;
            }
            else
            {
                dodgeDirection = 0;
            }

            // Separation from other enemies
            Vector2 separation = CalculateSeparation();

            // Calculate dodge offset
            Vector2 dodgeOffset = Vector2.zero;
            if (dodgeDirection != 0)
            {
                // Get perpendicular direction to player
                Vector2 perp = new Vector2(-dirToPlayer.y, dirToPlayer.x) * dodgeDirection;
                dodgeOffset = perp.normalized * dodgeStrength;
            }

            // Combine movement: toward player + dodge + separation
            Vector2 move = dirToPlayer + dodgeOffset + separation * separationStrength;

            // Move toward player if outside attack range
            if (distance > attackRange)
            {
                rb.MovePosition(rb.position + move.normalized * speed * Time.fixedDeltaTime);
            }

            // Position and rotate the triangle in front of the enemy
            if (attackTriangle != null)
            {
                float triangleOffset = 0.5f;
                attackTriangle.localPosition = dirToPlayer * triangleOffset;
                float angle = Mathf.Atan2(dirToPlayer.y, dirToPlayer.x) * Mathf.Rad2Deg;
                attackTriangle.localRotation = Quaternion.Euler(0, 0, angle - 90);
            }
        }
    }

    private Vector2 CalculateSeparation()
    {
        Vector2 separation = Vector2.zero;
        Collider2D[] hits = Physics2D.OverlapCircleAll(rb.position, separationRadius, LayerMask.GetMask("Enemy"));
        foreach (var hit in hits)
        {
            if (hit.gameObject != this.gameObject)
            {
                Vector2 away = (rb.position - (Vector2)hit.transform.position);
                float dist = away.magnitude;
                if (dist > 0)
                    separation += away / dist; // Weighted by distance
            }
        }
        return separation;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, separationRadius);
    }

    private void Update()
    {
        if (isDead) return;
        attackTimer -= Time.deltaTime;
    }

    public void TryAttackPlayer(PlayerInputHandler player)
    {
        if (attackTimer > 0f) return;
        player.ChangeHealth(-10f);
        attackTimer = attackCooldown;
    }

    public void TakeDamage(float amount)
    {
        if (isDead) return;
        health -= amount;
        if (health <= 0f) Die();
    }

    private void Die()
    {
        if (isDead) return;
        isDead = true;
        OnDeath?.Invoke();
        Destroy(gameObject);
    }
}