// ============================
// Blue_Enemy.cs
// ============================
using System;
using System.Collections;
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

    // === Obstacle Avoidance ===
    [Header("Obstacle Avoidance")]
    public float obstacleAvoidanceRadius = 1.2f;
    public float obstacleAvoidanceStrength = 2.0f;
    public LayerMask obstacleLayerMask = 0; // Assign in inspector (e.g. "Obstacles")

    // === Front Marker ===
    [Header("Front Marker")]
    public Transform frontMarker; // Assign in inspector (e.g., empty GameObject or sprite)

    // === Stun handling ===
    private Coroutine stunCoroutine;
    private bool isStunned = false;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.freezeRotation = true; // Prevent spinning
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

            // Obstacle avoidance
            Vector2 obstacleAvoidance = CalculateObstacleAvoidance();

            // Combine movement: toward player + dodge + separation + obstacle avoidance
            Vector2 move = dirToPlayer + dodgeOffset + separation * separationStrength + obstacleAvoidance * obstacleAvoidanceStrength;

            // Always move straight toward the front (player direction)
            Vector2 moveDirection = dirToPlayer;

            // If stunned, skip movement (stay in place)
            if (!isStunned && distance > attackRange)
            {
                rb.MovePosition(rb.position + moveDirection.normalized * speed * Time.fixedDeltaTime);
            }

            // Position and rotate the triangle in front of the enemy
            if (attackTriangle != null)
            {
                float triangleOffset = 0.5f;
                attackTriangle.localPosition = dirToPlayer * triangleOffset;
                float angle = Mathf.Atan2(dirToPlayer.y, dirToPlayer.x) * Mathf.Rad2Deg;
                attackTriangle.localRotation = Quaternion.Euler(0, 0, angle - 90);
            }

            // Rotate front marker to face the player
            if (frontMarker != null)
            {
                float frontAngle = Mathf.Atan2(dirToPlayer.y, dirToPlayer.x) * Mathf.Rad2Deg;
                frontMarker.localRotation = Quaternion.Euler(0, 0, frontAngle - 90);
                frontMarker.localPosition = dirToPlayer * 0.7f; // Place marker in front
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

    private Vector2 CalculateObstacleAvoidance()
    {
        Vector2 avoidance = Vector2.zero;
        Collider2D[] obstacles = Physics2D.OverlapCircleAll(rb.position, obstacleAvoidanceRadius, obstacleLayerMask);
        foreach (var obs in obstacles)
        {
            if (obs.gameObject != this.gameObject)
            {
                Vector2 away = (rb.position - (Vector2)obs.transform.position);
                float dist = away.magnitude;
                if (dist > 0)
                    avoidance += away / dist; // Weighted by distance
            }
        }
        return avoidance;
    }

    private void OnDrawGizmosSelected()
    {
        // Draw front direction line
        if (frontMarker != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(transform.position, frontMarker.position);
        }
        else
        {
            // If no marker, draw a default line forward
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(transform.position, transform.position + transform.up * 1.0f);
        }
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

    // Called to stun this enemy for 'seconds' seconds.
    public void OnStunned(float seconds)
    {
        if (stunCoroutine != null) StopCoroutine(stunCoroutine);
        stunCoroutine = StartCoroutine(StunRoutine(seconds));
    }

    private IEnumerator StunRoutine(float seconds)
    {
        isStunned = true;
        // Optionally clear movement-related timers so dodge/attack don't advance while stunned:
        dodgeDirection = 0;
        dodgeTimeLeft = 0f;
        yield return new WaitForSeconds(seconds);
        isStunned = false;
        stunCoroutine = null;
    }
}