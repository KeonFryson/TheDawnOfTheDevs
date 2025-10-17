using System.Collections.Generic;
using UnityEngine;

public class Grenade : MonoBehaviour
{
    [Header("Reference Data (assign a GrenadeData asset)")]
    public GrenadeData grenadeData;

    [Header("Impact")]
    [Tooltip("Explode immediately on heavy impact (ignored for Sticky and Smoke variants)")]
    public bool explodeOnImpact = true;
    [Tooltip("Relative velocity magnitude threshold to trigger impact explosion")]
    public float impactThreshold = 6f;

    // Fallback values (used if grenadeData is not assigned)
    public float fallbackFuseTime = 2.0f;
    public int fallbackDamage = 25;
    public float fallbackRadius = 2.0f;
    public float fallbackForce = 300f;

    private bool exploded = false;

    private void Start()
    {
        float fuse = grenadeData != null ? grenadeData.fuseTime : fallbackFuseTime;
        Invoke(nameof(Explode), fuse);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Impact detonation (frag/cluster/flash/default) unless configured otherwise
        if (explodeOnImpact && collision.relativeVelocity.magnitude >= impactThreshold)
        {
            var dataCheck = grenadeData;
            bool isSticky = dataCheck != null && dataCheck.variant == GrenadeVariant.Sticky;
            bool isSmoke = dataCheck != null && dataCheck.variant == GrenadeVariant.Smoke;

            if (!isSticky && !isSmoke)
            {
                CancelInvoke(nameof(Explode));
                Explode();
                return;
            }
        }

        // Sticky behaviour: attach to first collision and stop movement, then keep fuse running
        if (grenadeData != null && grenadeData.variant == GrenadeVariant.Sticky && !exploded)
        {
            var rb = GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.linearVelocity = Vector2.zero;
                rb.bodyType = RigidbodyType2D.Kinematic;
            }
            transform.SetParent(collision.transform, true);
        }
    }

    public void Explode()
    {
        if (exploded) return;
        exploded = true;

        var data = grenadeData;
        // choose config values
        int damage = data != null ? data.damage : fallbackDamage;
        float radius = data != null ? data.explosionRadius : fallbackRadius;
        float force = data != null ? data.explosionForce : fallbackForce;
        LayerMask mask = data != null ? data.damageLayerMask : ~0;

        // Common VFX / SFX spawn (some variants will override behavior)
        if (data != null && data.explosionEffect != null)
        {
            var eff = Instantiate(data.explosionEffect, transform.position, Quaternion.identity);
            var ps = eff.GetComponent<ParticleSystem>();
            if (ps != null) ps.Play();
            Destroy(eff, data.effectDuration);
        }
        if (data != null && data.explosionSound != null)
            AudioSource.PlayClipAtPoint(data.explosionSound, transform.position);

        // Variant-specific behavior
        GrenadeVariant variant = data != null ? data.variant : GrenadeVariant.Frag;
        switch (variant)
        {
            case GrenadeVariant.Smoke:
                HandleSmoke(data);
                break;
            case GrenadeVariant.Cluster:
                HandleCluster(data);
                break;
            case GrenadeVariant.Flashbang:
                HandleFlashbang(data, radius, mask);
                break;
            case GrenadeVariant.Sticky:
            case GrenadeVariant.Frag:
            default:
                HandleFragLike(data, damage, radius, force, mask);
                break;
        }

        // debug
        Debug.DrawLine(transform.position, transform.position + Vector3.right * radius, Color.red, 1f);

        Destroy(gameObject);
    }

    private void HandleFragLike(GrenadeData data, int damage, float radius, float force, LayerMask mask)
    {
        // Damage and physics impulse with falloff
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, radius, mask);
        var damaged = new HashSet<Blue_Enemy>();
        foreach (var hit in hits)
        {
            if (hit == null) continue;

            Rigidbody2D rb = hit.attachedRigidbody;
            if (rb != null)
            {
                Vector2 dir = ((Vector2)rb.position - (Vector2)transform.position);
                float dist = Mathf.Max(dir.magnitude, 0.01f);
                float falloff = Mathf.Clamp01(1f - (dist / radius));
                rb.AddForce(dir.normalized * force * falloff, ForceMode2D.Impulse);
            }

            Blue_Enemy enemy = hit.GetComponentInParent<Blue_Enemy>();
            if (enemy != null && !damaged.Contains(enemy))
            {
                float distE = Vector2.Distance(transform.position, enemy.transform.position);
                float dmgMul = Mathf.Clamp01(1f - (distE / radius));
                int applied = Mathf.Max(1, Mathf.RoundToInt(damage * dmgMul));
                enemy.TakeDamage(applied);
                damaged.Add(enemy);
            }
        }
    }

    private void HandleFlashbang(GrenadeData data, float radius, LayerMask mask)
    {
        // Flash: no direct damage. Notify targets in radius that they are stunned/blinded.
        float stun = data != null ? data.flashStunDuration : 1.5f;
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, radius, mask);
        foreach (var hit in hits)
        {
            if (hit == null) continue;

            // Notify via SendMessage so existing enemy scripts can respond by implementing OnStunned/OnBlinded
            hit.SendMessageUpwards("OnStunned", stun, SendMessageOptions.DontRequireReceiver);
            hit.SendMessageUpwards("OnBlinded", stun, SendMessageOptions.DontRequireReceiver);
        }
    }

    private void HandleSmoke(GrenadeData data)
    {
        // Spawn a smoke area prefab (expected to have a SmokeArea component that notifies overlap)
        if (data != null && data.smokePrefab != null)
        {
            var smoke = Instantiate(data.smokePrefab, transform.position, Quaternion.identity);
            // If SmokeArea is present, configure duration/radius if needed
            var smokeArea = smoke.GetComponent<SmokeArea>();
            if (smokeArea != null)
            {
                smokeArea.duration = data.effectDuration;
                smokeArea.radius = data.explosionRadius;
            }
            // let the smoke prefab manage its own lifetime
        }
    }

    private void HandleCluster(GrenadeData data)
    {
        if (data == null || data.clusterFragmentPrefab == null) return;

        for (int i = 0; i < data.clusterCount; i++)
        {
            float angle = (360f / data.clusterCount) * i + Random.Range(-data.clusterSpread, data.clusterSpread);
            Vector2 dir = new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad));
            var frag = Instantiate(data.clusterFragmentPrefab, transform.position, Quaternion.identity);
            var rb = frag.GetComponent<Rigidbody2D>();
            if (rb != null)
                rb.AddForce(dir * (data.throwForce * 0.5f), ForceMode2D.Impulse);
        }
    }

    private void OnDrawGizmosSelected()
    {
        float radius = grenadeData != null ? grenadeData.explosionRadius : fallbackRadius;
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, radius);
    }
}