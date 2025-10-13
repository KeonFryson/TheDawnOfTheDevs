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

    [Header("Held Weapon Prefab")]
    [SerializeField] GameObject startingWeaponPrefab;
    [SerializeField] WeaponStats startingWeaponStats;

    [SerializeField] private int baseDamage = 1;
    private int currentDamage;

    [System.Serializable]
    public class WeaponSlot
    {
        public WeaponType type;
        public GameObject prefab;
        public int ammo;
        public int maxAmmo;
        public WeaponStats stats;
    }

    public List<WeaponSlot> weaponSlots = new List<WeaponSlot>();
    private int currentWeaponSlot = 0;

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

        weaponSlots.Add(new WeaponSlot
        {
            type = WeaponType.Pistol,
            prefab = startingWeaponPrefab,
            ammo = startingWeaponStats != null ? startingWeaponStats.maxAmmo : 1,
            maxAmmo = startingWeaponStats != null ? startingWeaponStats.maxAmmo : 1,
            stats = startingWeaponStats
        });

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
        UpdateWeaponUI();
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
        return weaponSlots[currentWeaponSlot].type;
    }

    public bool AddWeaponToSlots(WeaponType newWeapon, GameObject weaponPrefab = null, int ammo = 1, int maxAmmo = 1, WeaponStats stats = null)
    {
        if (weaponSlots.Any(ws => ws.type == newWeapon))
            return false;

        if (weaponSlots.Count < 2)
        {
            weaponSlots.Add(new WeaponSlot
            {
                type = newWeapon,
                prefab = weaponPrefab,
                ammo = ammo,
                maxAmmo = maxAmmo,
                stats = stats
            });
            UpdateHeldWeaponVisual();
            UpdateWeaponUI();
            return true;
        }
        return false;
    }

    public void ReplaceWeaponInSlot(int slotIndex, WeaponType newWeapon, GameObject weaponPrefab = null, int ammo = 1, int maxAmmo = 1, WeaponStats stats = null)
    {
        if (slotIndex >= 0 && slotIndex < weaponSlots.Count)
        {
            weaponSlots[slotIndex] = new WeaponSlot
            {
                type = newWeapon,
                prefab = weaponPrefab,
                ammo = ammo,
                maxAmmo = maxAmmo,
                stats = stats
            };
            UpdateHeldWeaponVisual();
            UpdateWeaponUI();
        }
    }

    private void UpdateHeldWeaponVisual()
    {
        if (currentHeldWeaponInstance != null)
        {
            Destroy(currentHeldWeaponInstance);
            currentHeldWeaponInstance = null;
        }

        WeaponSlot slot = weaponSlots.Count > currentWeaponSlot ? weaponSlots[currentWeaponSlot] : null;
        if (slot != null && slot.prefab != null && weaponHolder != null)
        {
            currentHeldWeaponInstance = Instantiate(slot.prefab, weaponHolder.position, weaponHolder.rotation, weaponHolder);
            weaponFirePoint = currentHeldWeaponInstance.transform.Find("FirePoint");
        }
    }

    public void Shoot(Vector3 playerPosition)
    {
        WeaponSlot slot = weaponSlots[currentWeaponSlot];
        if (slot.ammo <= 0)
        {
            AutoSwitchWeaponIfOutOfAmmo();
            return;
        }

        WeaponType currentWeapon = slot.type;
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(UnityEngine.InputSystem.Mouse.current.position.ReadValue());
        mouseWorldPos.z = playerPosition.z;
        Vector2 shootDir = (mouseWorldPos - playerPosition).normalized;
        int damageToSet = (slot.stats != null ? slot.stats.damage : GetCurrentDamage()) + (currentDamage - baseDamage);

        Transform fireTransform = weaponFirePoint != null ? weaponFirePoint : weaponHolder;

        // --- Muzzle Flash ---
        if (muzzleFlashPrefab != null && fireTransform != null)
        {
            Vector3 flashPosition = fireTransform.position + (Vector3)(shootDir * 0.6f);
            GameObject flash = Instantiate(muzzleFlashPrefab, flashPosition, fireTransform.rotation, fireTransform);
            flash.transform.localScale = new Vector3(0.1f, 0.1f, 1f);
            Destroy(flash, 0.1f);
        }

        switch (currentWeapon)
        {
            case WeaponType.Pistol:
                var pistolBullet = Instantiate(pistolBulletPrefab, fireTransform.position, fireTransform.rotation);
                var pistolBulletComponent = pistolBullet.GetComponent<Bullet>();
                pistolBulletComponent.SetDirection(shootDir);
                pistolBulletComponent.SetDamage(damageToSet);
                slot.ammo--;
                break;
            case WeaponType.Shotgun:
                float spreadAngle = slot.stats != null ? slot.stats.spreadAngle : 15f;
                int pelletCount = slot.stats != null ? slot.stats.pelletCount : 5;
                for (int i = 0; i < pelletCount; i++)
                {
                    float angle = -spreadAngle * 0.5f + spreadAngle * i / (pelletCount - 1);
                    Vector2 dir = Quaternion.Euler(0, 0, angle) * shootDir;
                    var shotgunBullet = Instantiate(shotgunBulletPrefab, fireTransform.position, fireTransform.rotation);
                    var shotgunBulletComponent = shotgunBullet.GetComponent<Bullet>();
                    shotgunBulletComponent.SetDirection(dir);
                    shotgunBulletComponent.SetDamage(damageToSet);
                }
                slot.ammo--;
                break;
            case WeaponType.Laser:
                slot.ammo--;
                break;
        }

        UpdateWeaponUI();
        AutoSwitchWeaponIfOutOfAmmo();
    }

    public void OnAttackPressed(Vector3 playerPosition)
    {
        WeaponSlot slot = weaponSlots[currentWeaponSlot];
        WeaponType currentWeapon = slot.type;
        if (slot.ammo <= 0)
        {
            AutoSwitchWeaponIfOutOfAmmo();
            return;
        }

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
        WeaponSlot slot = weaponSlots[currentWeaponSlot];
        WeaponType currentWeapon = slot.type;
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
            WeaponSlot slot = weaponSlots[currentWeaponSlot];
            slot.ammo--;
            UpdateWeaponUI();
            if (slot.ammo <= 0)
            {
                AutoSwitchWeaponIfOutOfAmmo();
                yield break;
            }
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
            if (hit.collider != null && hit.collider.gameObject == gameObject)
                continue;

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

    public void UpdateWeaponUI()
    {
        var weaponUI = FindFirstObjectByType<WeaponUI>();
        if (weaponUI != null)
        {
            weaponUI.UpdateWeaponDisplay(weaponSlots[currentWeaponSlot]);
        }
    }

    public void SwitchWeapon()
    {
        if (weaponSlots[currentWeaponSlot].type == WeaponType.Laser && laserCoroutine != null)
        {
            StopCoroutine(laserCoroutine);
            laserCoroutine = null;
            if (laserLineRenderer != null)
                laserLineRenderer.enabled = false;
        }
        currentWeaponSlot = (currentWeaponSlot + 1) % weaponSlots.Count;
        UpdateHeldWeaponVisual();
        UpdateWeaponUI();
    }

  s
}