
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(CircleCollider2D))]
public class SmokeArea : MonoBehaviour
{
    [Tooltip("How long the smoke lasts (seconds)")]
    public float duration = 5f;
    [Tooltip("Radius of the smoke area (used to set collider)")]
    public float radius = 2f;

    private CircleCollider2D cc;

    private void Awake()
    {
        cc = GetComponent<CircleCollider2D>();
        cc.isTrigger = true;
        cc.radius = radius;
    }

    private void Start()
    {
        StartCoroutine(Life());
    }

    private IEnumerator Life()
    {
        yield return new WaitForSeconds(duration);
        Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        other.SendMessageUpwards("OnEnterSmoke", SendMessageOptions.DontRequireReceiver);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        other.SendMessageUpwards("OnExitSmoke", SendMessageOptions.DontRequireReceiver);
    }
}