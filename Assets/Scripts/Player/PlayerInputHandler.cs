using System.ComponentModel;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputHandler : MonoBehaviour
{
    [SerializeField] float BaseSpeed = 5f;
    private float MovementSpeed;

    [SerializeField] Animator Animator;
    [SerializeField] SpriteRenderer SpriteRenderer;
    private Vector2 m_moveInput;
    private PlayerControls m_controls;

    private void Awake()
    {
       
        m_controls = new PlayerControls();


        //Movement
        m_controls.Player.Move.performed += ctx => m_moveInput = ctx.ReadValue<Vector2>();
        m_controls.Player.Move.canceled += ctx => m_moveInput = Vector2.zero;

    }
    private void OnEnable()
    {
        m_controls.Enable();
    }

    private void OnDisable()
    {
        m_controls.Disable();
    }

    private void Update()
    {
        MovementSpeed = BaseSpeed;

        Vector3 movement = new Vector3(m_moveInput.x, m_moveInput.y, 0f);
        transform.position += MovementSpeed * Time.deltaTime * movement;

        //Animator.SetFloat("speed", movement.magnitude);

        if (movement.x > 0) SpriteRenderer.flipX = false;
        else if (movement.x < 0) SpriteRenderer.flipX = true;
    }
}
