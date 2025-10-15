using UnityEngine;

[System.Serializable]
public class WeaponStats
{
    public int damage = 1;
    public int maxAmmo = 100;
    public int maxClipAmmo = 10; // Add this line
    public float fireRate = 0.5f;
    public float spreadAngle = 0f;
    public int pelletCount = 1;
    public float range = 10f;
    
}