using System.ComponentModel;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputHandler : MonoBehaviour
{
    [SerializeField] float BaseSpeed = 5f;
    private float MovementSpeed;

    [SerializeField] float Health = 100f;
    [SerializeField] float MaxHealth = 100f;

    [SerializeField] Animator Animator;
    [SerializeField] SpriteRenderer SpriteRenderer;
    private Vector2 m_moveInput;
    private InputSystem_Actions m_controls;

    private Rigidbody2D rb;

    // Weapon system
    private enum WeaponType { Pistol, Shotgun }
    private WeaponType currentWeapon = WeaponType.Pistol;

    [Header("Weapon Prefabs")]
    [SerializeField] GameObject pistolBulletPrefab;
    [SerializeField] GameObject shotgunBulletPrefab;
    [SerializeField] Transform firePoint;

    private void Awake()
    {

        m_controls = new InputSystem_Actions();
        rb = GetComponent<Rigidbody2D>();

        // Freeze rotation so player doesn't spin on collision
        rb.freezeRotation = true;

        //Movement
        m_controls.Player.Move.performed += ctx => m_moveInput = ctx.ReadValue<Vector2>();
        m_controls.Player.Move.canceled += ctx => m_moveInput = Vector2.zero;

        // Shooting
        m_controls.Player.Attack.performed += ctx => Shoot();

        // Weapon switching
        m_controls.Player.SwitchWeapon.performed += ctx => SwitchWeapon();

    }
    private void OnEnable()
    {
        m_controls.Enable();
    }

    private void OnDisable()
    {
        m_controls.Disable();
    }

    private void FixedUpdate()
    {
        MovementSpeed = BaseSpeed;
        Vector2 movement = m_moveInput * MovementSpeed * Time.fixedDeltaTime;
        rb.MovePosition(rb.position + movement);

        if (movement.x > 0) SpriteRenderer.flipX = false;
        else if (movement.x < 0) SpriteRenderer.flipX = true;


    }

    private void Update()
    {

        // --- FirePoint rotation and position ---
        // Get mouse position in world space
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        mouseWorldPos.z = transform.position.z; // Keep player z for 2D

        // Direction from player to mouse
        Vector3 dirToMouse = (mouseWorldPos - transform.position).normalized;

        // Set firePoint at fixed radius from player (e.g., 0.7 units)
        float firePointRadius = 7f;
        firePoint.position = transform.position + dirToMouse * firePointRadius;

        // Rotate firePoint to face mouse direction
        float angle = Mathf.Atan2(dirToMouse.y, dirToMouse.x) * Mathf.Rad2Deg;
        firePoint.rotation = Quaternion.Euler(0, 0, angle);

        if (Health <= 0)
        {
            Destroy(gameObject);
        }
    }
    // Health change logic
    public void ChangeHealth(float amount)
    {
        Health = Mathf.Clamp(Health + amount, 0, MaxHealth);


        // Optionally handle death
        if (Health <= 0)
        {
            // You can add respawn or death logic here
            Destroy(gameObject);
        }
    }



    private void Shoot()
    {
        // Direction from player to mouse
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        mouseWorldPos.z = transform.position.z;
        Vector2 shootDir = (mouseWorldPos - transform.position).normalized;

        switch (currentWeapon)
        {
            case WeaponType.Pistol:
                var pistolBullet = Instantiate(pistolBulletPrefab, firePoint.position, Quaternion.identity);
                pistolBullet.GetComponent<Bullet>().SetDirection(shootDir);
                break;
            case WeaponType.Shotgun:
                float spreadAngle = 15f;
                int pelletCount = 5;
                for (int i = 0; i < pelletCount; i++)
                {
                    float angle = -spreadAngle * 0.5f + spreadAngle * i / (pelletCount - 1);
                    Vector2 dir = Quaternion.Euler(0, 0, angle) * shootDir;
                    var shotgunBullet = Instantiate(shotgunBulletPrefab, firePoint.position, Quaternion.identity);
                    shotgunBullet.GetComponent<Bullet>().SetDirection(dir);
                }
                break;
        }
    }

    private void SwitchWeapon()
    {
        currentWeapon = currentWeapon == WeaponType.Pistol ? WeaponType.Shotgun : WeaponType.Pistol;
        // Optionally, update UI or play sound here
    }
}
