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

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        timer = changeDirTimer;
    }

    private void FixedUpdate()
    {
        rb.MovePosition(rb.position + moveDir * speed * Time.fixedDeltaTime);
    }

    private void Update()
    {
        timer -= Time.deltaTime;
        if (timer <= 0f)
        {
            moveDir *= -1;
            timer = changeDirTimer;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.TryGetComponent<PlayerInputHandler>(out var player))
        {
            player.ChangeHealth(-10f);
        }
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
