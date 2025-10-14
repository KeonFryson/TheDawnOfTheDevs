using System.Collections;
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

    [Header("Weapon Handler")]
    [SerializeField] private PlayerWeaponHandler weaponHandler;

    [Header("Footstep Audio")]
    [SerializeField] private AudioClip[] footstepClips;
    [SerializeField] private AudioSource footstepSource;
    [SerializeField] private float footstepInterval = 0.4f;
    private float footstepTimer = 0f;

    private void Awake()
    {
        m_controls = new InputSystem_Actions();
        rb = GetComponent<Rigidbody2D>();
        rb.freezeRotation = true;

        m_controls.Player.Move.performed += ctx => m_moveInput = ctx.ReadValue<Vector2>();
        m_controls.Player.Move.canceled += ctx => m_moveInput = Vector2.zero;

        m_controls.Player.Attack.performed += ctx => weaponHandler.OnAttackPressed(transform.position);
        m_controls.Player.Attack.canceled += ctx => weaponHandler.OnAttackReleased();

        m_controls.Player.SwitchWeapon.performed += ctx => weaponHandler.SwitchWeapon();
        m_controls.Player.Reload.performed += ctx => weaponHandler.ReloadCurrentWeapon();
    }

    private void OnEnable()
    {
        m_controls.Enable();
    }

    private void OnDisable()
    {
        m_controls.Disable();
    }

    public void SetInputEnabled(bool enabled)
    {
        if (enabled)
            m_controls.Enable();
        else
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
        // Footstep audio logic
        footstepTimer -= Time.deltaTime;
        if (m_moveInput.magnitude > 0.1f && footstepTimer <= 0f)
        {
            PlayFootstep();
            footstepTimer = footstepInterval;
        }

        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        mouseWorldPos.z = transform.position.z;

        if (weaponHandler.WeaponHolder != null)
        {
            Vector3 dirToMouse = (mouseWorldPos - transform.position).normalized;
            float orbitRadius = 4.0f;
            weaponHandler.WeaponHolder.position = transform.position + dirToMouse * orbitRadius;

            float angle = Mathf.Atan2(dirToMouse.y, dirToMouse.x) * Mathf.Rad2Deg;

            int Scale = 3;
            // Flip weapon Y scale when aiming left
            if (weaponHandler.WeaponHolder.childCount > 0)
            {
                Transform weapon = weaponHandler.WeaponHolder.GetChild(0);
                if (dirToMouse.x < 0)
                    weapon.localScale = new Vector3(Scale, Scale, Scale);
                else
                    weapon.localScale = new Vector3(Scale, -Scale, Scale);
            }

            weaponHandler.WeaponHolder.rotation = Quaternion.Euler(0, 0, angle - 180);
        }

        weaponHandler.UpdateLaserVisual(transform.position);

        if (Health <= 0)
        {
            Destroy(gameObject);
        }
    }

    private void PlayFootstep()
    {
        if (footstepClips != null && footstepClips.Length > 0 && footstepSource != null)
        {
            int idx = Random.Range(0, footstepClips.Length);
            footstepSource.PlayOneShot(footstepClips[idx]);
        }
    }

    public void ChangeHealth(float amount)
    {
        Health = Mathf.Clamp(Health + amount, 0, MaxHealth);
        if (Health <= 0)
        {
            Destroy(gameObject);
        }
    }

    public void ChangeMaxHealth(float amount)
    {
        MaxHealth += amount;
        MaxHealth = Mathf.Max(MaxHealth, 100f);
        Health = Mathf.Clamp(Health, 0, MaxHealth);
        if (Health <= 0)
        {
            Destroy(gameObject);
        }
    }

    public void IncreaseDamage(int amount)
    {
        weaponHandler.IncreaseDamage(amount);
    }

    public int GetCurrentDamage()
    {
        return weaponHandler.GetCurrentDamage();
    }
}