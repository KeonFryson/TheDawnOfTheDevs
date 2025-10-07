using UnityEngine;

public class Bullet : MonoBehaviour
{
    [SerializeField] float speed = 50f;
    [SerializeField] float lifetime = 1f;
    [SerializeField] int damage;
    private Vector2 direction = Vector2.right;
    private Rigidbody2D rb;

    public void SetDamage(int value)
    {
        damage = value;
    }
    public void SetDirection(Vector2 dir)
    {
        direction = dir.normalized;
    }

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void Start()
    {
        rb.linearVelocity = direction * (speed * 2) ;
        Destroy(gameObject, lifetime);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.GetComponent<Bullet>() != null)
            return;
        // Ignore AttackTriangle by layer
        if (collision.gameObject.layer == LayerMask.NameToLayer("Ignore Raycast"))
            return;


        Blue_Enemy blueEnemy = collision.GetComponent<Blue_Enemy>();
        if (blueEnemy != null)
        {
            blueEnemy.TakeDamage(damage);
        }
        // Add damage logic here if needed
        Destroy(gameObject);
    }
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.GetComponent<Bullet>() != null)
            return;
        // Ignore AttackTriangle by layer
        if (collision.gameObject.layer == LayerMask.NameToLayer("Ignore Raycast"))
            return;
        Blue_Enemy blueEnemy = collision.collider.GetComponent<Blue_Enemy>();
        if (blueEnemy != null)
        {
            blueEnemy.TakeDamage(damage);
        }
        // Add damage logic here if needed
        Destroy(gameObject);
    }
}