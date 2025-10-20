using System;
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Blue_Enemy : MonoBehaviour
{
    public float speed = 2f;
    public float health = 20f;

    // Added so damage can be scaled per-wave
    public float attackDamage = 10f;

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
    public LayerMask obstacleLayerMask = ~0; // default to all layers (will be cleaned in Awake)

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

        // If inspector left mask at default (0) or user didn't assign, default to all layers
        // then clear Player and Enemy layers if they exist so those are not treated as obstacles.
        int mask = obstacleLayerMask.value;
        if (mask == 0)
            mask = ~0; // all layers

        int playerLayer = LayerMask.NameToLayer("Player");
        if (playerLayer != -1)
            mask &= ~(1 << playerLayer);

        int enemyLayer = LayerMask.NameToLayer("Enemy");
        if (enemyLayer != -1)
            mask &= ~(1 << enemyLayer);

        obstacleLayerMask.value = mask;
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
            Vector2 combinedMove = dirToPlayer + dodgeOffset + separation * separationStrength + obstacleAvoidance * obstacleAvoidanceStrength;

            // Use the combined move vector for actual motion (was only using direct player direction before,
            // which caused the enemy to push into obstacles and get stuck).
            Vector2 desiredMove = combinedMove.sqrMagnitude > 0.0001f ? combinedMove.normalized : dirToPlayer;

            // --- Simple collision check ahead and slide along obstacle normal if blocked ---
            // This helps the enemy not get stuck when a direct move would collide with an obstacle.
            Vector2 finalMoveDir = desiredMove;
            Collider2D ownCol = GetComponent<Collider2D>();
            float castRadius = ownCol != null ? Mathf.Max(ownCol.bounds.extents.x, ownCol.bounds.extents.y) : 0.25f;
            float castDistance = speed * Time.fixedDeltaTime + 0.01f;
            RaycastHit2D hit = Physics2D.CircleCast(rb.position, castRadius, desiredMove, castDistance, obstacleLayerMask);
            if (hit.collider != null)
            {
                // Slide along the obstacle surface: project movement onto tangent
                Vector2 tangent = new Vector2(-hit.normal.y, hit.normal.x);
                float alongTangent = Vector2.Dot(desiredMove, tangent);
                // if tangent contributes, prefer sliding; otherwise try slight away from obstacle
                if (Mathf.Abs(alongTangent) > 0.01f)
                    finalMoveDir = tangent.normalized * Mathf.Sign(alongTangent);
                else
                    finalMoveDir = (desiredMove + hit.normal * 0.6f).normalized;
            }

            // If stunned, skip movement (stay in place)
            if (!isStunned && distance > attackRange)
            {
                rb.MovePosition(rb.position + finalMoveDir * speed * Time.fixedDeltaTime);
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
            if (obs.gameObject == this.gameObject)
                continue;

            // Ignore any collider that belongs to the player (including tail child colliders)
            if (obs.GetComponentInParent<PlayerInputHandler>() != null)
                continue;

            // Ignore trigger colliders (they shouldn't block movement)
            if (obs.isTrigger)
                continue;

            Vector2 away = (rb.position - (Vector2)obs.transform.position);
            float dist = away.magnitude;
            if (dist > 0)
                avoidance += away / dist; // Weighted by distance
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

        if (obstacleAvoidanceRadius > 0f)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, obstacleAvoidanceRadius);
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
        player.ChangeHealth(-attackDamage);
        // record that player received damage
        GameStats.RecordDamageReceived(attackDamage);
        attackTimer = attackCooldown;
    }

    public void TakeDamage(float amount)
    {
        if (isDead) return;
        health -= amount;
        // record damage dealt by player to enemies
        GameStats.RecordDamageDealt(amount);
        if (health <= 0f) Die();
    }

    private void Die()
    {
        if (isDead) return;
        isDead = true;
        // record enemy death/score
        GameStats.RecordEnemyKilled();
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