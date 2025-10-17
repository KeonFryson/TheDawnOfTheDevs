using UnityEngine;

public class Damge : MonoBehaviour
{
    [SerializeField] private int damageAmount = 1;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        DoDamage(collision.gameObject, damageAmount);


    }
    private void OnCollisionEnter2D(Collision2D collision)
    {
        DoDamage(collision.gameObject, damageAmount);
    }

    private void DoDamage(GameObject obj, int damage)
    {
        //IDamageable damageable = obj.GetComponent<IDamageable>();
        //damageable?.TakeDamage(damage);
    }
}
