using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlayerWeaponHandler : MonoBehaviour
{
    public enum WeaponType { Pistol, Shotgun, Laser }

    [Header("Weapon Prefabs")]
    [SerializeField] GameObject pistolBulletPrefab;
    [SerializeField] GameObject shotgunBulletPrefab;
    //[SerializeField] Transform firePoint;
    //public Transform FirePoint => firePoint;

    [Header("Held Weapon Prefab")]
    [SerializeField] GameObject startingWeaponPrefab;

    [SerializeField] private int baseDamage = 1;
    private int currentDamage;

    public List<WeaponType> weaponSlots = new List<WeaponType> { WeaponType.Pistol };
    private int currentWeaponSlot = 0;

    // NEW: Store weapon prefab references for each slot
    private List<GameObject> weaponPrefabSlots = new List<GameObject>();
    private GameObject currentHeldWeaponInstance;
    [SerializeField] private Transform weaponHolder;
    [SerializeField] private GameObject muzzleFlashPrefab;
    public Transform WeaponHolder => weaponHolder;
    [Header("Laser Settings")]
    [SerializeField] private float laserFireRate = 0.1f;
    [SerializeField] private float laserRange = 100f;
    [SerializeField] private LineRenderer laserLineRenderer;
    private Coroutine laserCoroutine;
    private bool isLaserActive = false;

    private Transform weaponFirePoint;
    private void Awake()
    {
        currentDamage = baseDamage;
        // Ensure weaponPrefabSlots matches weaponSlots count
        weaponPrefabSlots.Add(startingWeaponPrefab); // <-- Use the assigned prefab for the first weapon
        if (laserLineRenderer != null)
        {
            laserLineRenderer.startWidth = 2f;
            laserLineRenderer.endWidth = 2f;
            Gradient gradient = new Gradient();
            gradient.SetKeys(
                new GradientColorKey[] {
                new GradientColorKey(Color.cyan, 0.0f),
                new GradientColorKey(Color.white, 1.0f)
                },
                new GradientAlphaKey[] {
                new GradientAlphaKey(1.0f, 0.0f),
                new GradientAlphaKey(0.0f, 1.0f)
                }
            );
            laserLineRenderer.colorGradient = gradient;
        }
        UpdateHeldWeaponVisual();
    }


    public void IncreaseDamage(int amount)
    {
        currentDamage += amount;
    }

    public int GetCurrentDamage()
    {
        return currentDamage;
    }

    public WeaponType GetCurrentWeapon()
    {
        return weaponSlots[currentWeaponSlot];
    }
    // NEW: Add weapon with prefab
    public bool AddWeaponToSlots(WeaponType newWeapon, GameObject weaponPrefab = null)
    {
        // Prevent duplicates
        if (weaponSlots.Contains(newWeapon))
            return false;

        // Only allow up to 2 weapons
        if (weaponSlots.Count < 2)
        {
            weaponSlots.Add(newWeapon);
            weaponPrefabSlots.Add(weaponPrefab);
            UpdateHeldWeaponVisual();
            return true;
        }
        return false;
    }

    // NEW: Replace weapon in slot (for weapon replace dialog)
    public void ReplaceWeaponInSlot(int slotIndex, WeaponType newWeapon, GameObject weaponPrefab = null)
    {
        if (slotIndex >= 0 && slotIndex < weaponSlots.Count)
        {
            weaponSlots[slotIndex] = newWeapon;
            weaponPrefabSlots[slotIndex] = weaponPrefab;
            UpdateHeldWeaponVisual();
        }
    }

    // NEW: Update held weapon visual
    private void UpdateHeldWeaponVisual()
    {
        if (currentHeldWeaponInstance != null)
        {
            Destroy(currentHeldWeaponInstance);
            currentHeldWeaponInstance = null;
        }

        GameObject prefab = weaponPrefabSlots.Count > currentWeaponSlot ? weaponPrefabSlots[currentWeaponSlot] : null;
        if (prefab != null && weaponHolder != null)
        {
            currentHeldWeaponInstance = Instantiate(prefab, weaponHolder.position, weaponHolder.rotation, weaponHolder);
            weaponFirePoint = currentHeldWeaponInstance.transform.Find("FirePoint");
        }
    }

    public void Shoot(Vector3 playerPosition)
    {
        WeaponType currentWeapon = weaponSlots[currentWeaponSlot];
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(UnityEngine.InputSystem.Mouse.current.position.ReadValue());
        mouseWorldPos.z = playerPosition.z;
        Vector2 shootDir = (mouseWorldPos - playerPosition).normalized;
        int damageToSet = GetCurrentDamage();

        Transform fireTransform = weaponFirePoint != null ? weaponFirePoint : weaponHolder;

        // --- Muzzle Flash ---
        if (muzzleFlashPrefab != null && fireTransform != null)
        {
            Vector3 flashPosition = fireTransform.position + (Vector3)(shootDir * 0.6f); // Slightly in front of fire point
            GameObject flash = Instantiate(muzzleFlashPrefab, flashPosition, fireTransform.rotation, fireTransform);
            flash.transform.localScale = new Vector3(0.1f, 0.1f, 1f); // Adjust scale and flip if needed
            Destroy(flash, 0.1f); // Adjust duration as needed
        }

        switch (currentWeapon)
        {
            case WeaponType.Pistol:
                var pistolBullet = Instantiate(pistolBulletPrefab, fireTransform.position, fireTransform.rotation);
                var pistolBulletComponent = pistolBullet.GetComponent<Bullet>();
                pistolBulletComponent.SetDirection(shootDir);
                pistolBulletComponent.SetDamage(damageToSet);
                break;
            case WeaponType.Shotgun:
                float spreadAngle = 15f;
                int pelletCount = 5;
                for (int i = 0; i < pelletCount; i++)
                {
                    float angle = -spreadAngle * 0.5f + spreadAngle * i / (pelletCount - 1);
                    Vector2 dir = Quaternion.Euler(0, 0, angle) * shootDir;
                    var shotgunBullet = Instantiate(shotgunBulletPrefab, fireTransform.position, fireTransform.rotation);
                    var shotgunBulletComponent = shotgunBullet.GetComponent<Bullet>();
                    shotgunBulletComponent.SetDirection(dir);
                    shotgunBulletComponent.SetDamage(damageToSet);
                }
                break;
            case WeaponType.Laser:
                // No projectile for raycast laser
                break;
        }
    }
    public void OnAttackPressed(Vector3 playerPosition)
    {
        WeaponType currentWeapon = weaponSlots[currentWeaponSlot];
        if (currentWeapon == WeaponType.Laser)
        {
            if (laserCoroutine == null)
                laserCoroutine = StartCoroutine(FireLaserRaycast(playerPosition));
            isLaserActive = true;
            if (laserLineRenderer != null)
                laserLineRenderer.enabled = true;
        }
        else
        {
            Shoot(playerPosition);
        }
    }

    public void OnAttackReleased()
    {
        WeaponType currentWeapon = weaponSlots[currentWeaponSlot];
        if (currentWeapon == WeaponType.Laser && laserCoroutine != null)
        {
            StopCoroutine(laserCoroutine);
            laserCoroutine = null;
            isLaserActive = false;
            if (laserLineRenderer != null)
                laserLineRenderer.enabled = false;
        }
    }

    private IEnumerator FireLaserRaycast(Vector3 playerPosition)
    {
        while (true)
        {
            FireLaser(playerPosition);
            yield return new WaitForSeconds(laserFireRate);
        }
    }

    private void FireLaser(Vector3 playerPosition)
    {
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(UnityEngine.InputSystem.Mouse.current.position.ReadValue());
        mouseWorldPos.z = playerPosition.z;
        Vector2 shootDir = (mouseWorldPos - playerPosition).normalized;
        Vector3 laserOrigin = weaponFirePoint != null ? weaponFirePoint.position : weaponHolder.position;
        RaycastHit2D[] hits = Physics2D.RaycastAll(laserOrigin, shootDir, laserRange);
        foreach (var hit in hits)
        {
            // Ignore self (player)
            if (hit.collider != null && hit.collider.gameObject == gameObject)
                continue;

            // Log what the laser is hitting
            Debug.Log($"Laser hit: {hit.collider.name} (Tag: {hit.collider.tag}, Layer: {LayerMask.LayerToName(hit.collider.gameObject.layer)})");

            Blue_Enemy enemy = hit.collider.GetComponentInParent<Blue_Enemy>();
            if (enemy != null && hit.collider.gameObject == enemy.gameObject)
            {
                enemy.TakeDamage(currentDamage * 2);
                Debug.Log("Laser damaged enemy: " + enemy.name);
            }
        }
    }

    public void UpdateLaserVisual(Vector3 playerPosition)
    {
        if (!isLaserActive || laserLineRenderer == null)
            return;

        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(UnityEngine.InputSystem.Mouse.current.position.ReadValue());
        mouseWorldPos.z = playerPosition.z;
        Vector2 shootDir = (mouseWorldPos - playerPosition).normalized;

        Vector3 startPoint = weaponFirePoint != null ? weaponFirePoint.position : weaponHolder.position;
        Vector3 endPoint = startPoint + (Vector3)shootDir * laserRange;

        RaycastHit2D hit = Physics2D.Raycast(startPoint, shootDir, laserRange);
        if (hit.collider != null)
        {
            endPoint = hit.point;
        }

        float minWidth = 2.5f;
        float maxWidth = 3.5f;
        float pulse = Mathf.PingPong(Time.time * 10f, maxWidth - minWidth) + minWidth;
        laserLineRenderer.startWidth = pulse;
        laserLineRenderer.endWidth = pulse;

        Color flickerColor = Color.Lerp(Color.cyan, Color.white, Mathf.PingPong(Time.time * 2f, 1f));
        laserLineRenderer.startColor = flickerColor;
        laserLineRenderer.endColor = flickerColor;

        laserLineRenderer.SetPosition(0, startPoint);
        laserLineRenderer.SetPosition(1, endPoint);
    }

    public bool AddWeaponToSlots(WeaponType newWeapon)
    {
        // Prevent duplicates
        if (weaponSlots.Contains(newWeapon))
            return false;

        // Only allow up to 2 weapons
        if (weaponSlots.Count < 2)
        {
            weaponSlots.Add(newWeapon);
            return true;
        }
        return false;
    }

    public void SwitchWeapon()
    {
        if (weaponSlots[currentWeaponSlot] == WeaponType.Laser && laserCoroutine != null)
        {
            StopCoroutine(laserCoroutine);
            laserCoroutine = null;
            if (laserLineRenderer != null)
                laserLineRenderer.enabled = false;
        }
        currentWeaponSlot = (currentWeaponSlot + 1) % weaponSlots.Count;
        UpdateHeldWeaponVisual();
    }
}