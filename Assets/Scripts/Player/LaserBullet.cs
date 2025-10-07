using UnityEngine;

public class LaserBullet : MonoBehaviour
{
    public float speed = 30f;
    public float lifetime = 1.5f;
    private Vector2 direction;
    private int damage;

    public void SetDirection(Vector2 dir)
    {
        direction = dir.normalized;
    }

    public void SetDamage(int dmg)
    {
        damage = dmg;
    }

    void Start()
    {
        Destroy(gameObject, lifetime);
    }

    void Update()
    {
        transform.position += (Vector3)direction * speed * Time.deltaTime;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // Example: Damage enemy
        var enemy = other.GetComponent<Blue_Enemy>();
        if (enemy != null)
        {
            enemy.TakeDamage(damage);
            Destroy(gameObject);
        }
        else if (!other.isTrigger)
        {
            Destroy(gameObject);
        }
    }
}