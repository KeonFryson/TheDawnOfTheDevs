using UnityEngine;

public enum GrenadeVariant
{
    Frag,
    Flashbang,
    Smoke,
    Sticky,
    Cluster
}

[CreateAssetMenu(menuName = "Grenades/Grenade Data")]
public class GrenadeData : ScriptableObject
{
    public string displayName = "Grenade";
    public Sprite icon;
    public GrenadeVariant variant = GrenadeVariant.Frag;

    [Header("Timing / Physics")]
    public float fuseTime = 2.0f;
    public float upwardBias = 0.5f;
    public float throwForce = 8f;

    [Header("Explosion / Damage")]
    public int damage = 25;
    public float explosionRadius = 2.0f;
    public float explosionForce = 300f;
    public LayerMask damageLayerMask = ~0;

    [Header("Cluster (for Cluster variant)")]
    public int clusterCount = 6;
    public GameObject clusterFragmentPrefab;
    public float clusterSpread = 2.0f;

    [Header("Flashbang")]
    public float flashStunDuration = 2.0f;

    [Header("VFX / SFX")]
    public GameObject explosionEffect;
    public GameObject smokePrefab; // for Smoke variant
    public AudioClip explosionSound;
    public float effectDuration = 1.5f;
}