using UnityEngine;

public class Bullet : MonoBehaviour
{
    [SerializeField] float speed = 50f;
    [SerializeField] float lifetime = 1f;
    [SerializeField] int damage;
    private Vector2 direction = Vector2.right;

    public void SetDamage(int value)
    {
        damage = value;
    }
    public void SetDirection(Vector2 dir)
    {
        direction = dir.normalized;
    }

    private void Start()
    {
        Destroy(gameObject, lifetime);
    }

    private void Update()
    {
        transform.position += (Vector3)direction * speed * Time.deltaTime;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.GetComponent<Bullet>() != null)
            return;
        if (collision.gameObject.layer == LayerMask.NameToLayer("Ignore Raycast"))
            return;

        Blue_Enemy blueEnemy = collision.GetComponent<Blue_Enemy>();
        if (blueEnemy != null)
        {
            blueEnemy.TakeDamage(damage);
        }
        Destroy(gameObject);
    }
}