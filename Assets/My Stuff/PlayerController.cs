using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;

/// <summary>
/// Handles player-specific logic and connects inputs to movement/attack systems.
/// </summary>
[RequireComponent(typeof(PlayerMovement))]
//[RequireComponent(typeof(AudioSource))]
public class PlayerController : BaseEntity
{
    [Header("Player Settings")]
    [SerializeField] private bool controlEnabled = true;

    private PlayerMovement movement;
    //private AudioSource audioSource;
    private Animator animator;
    private SpriteRenderer sprite;

    private InputAction moveAction;
    private InputAction jumpAction;

    protected override void Awake()
    {
        base.Awake();
        movement = GetComponent<PlayerMovement>();
        audioSource = GetComponent<AudioSource>();
        animator = GetComponent<Animator>();
        sprite = GetComponent<SpriteRenderer>();

        var map = InputSystem.actions;
        moveAction = map.FindAction("Player/Move");
        jumpAction = map.FindAction("Player/Jump");

        OnEnable();
    }

    override protected void Update()
    {
        if (!controlEnabled || !IsAlive) return;

        Vector2 input = moveAction.ReadValue<Vector2>();
        movement.SetMoveInput(input.x);

        if (jumpAction.WasPressedThisFrame())
            movement.TryJump();

        if (jumpAction.WasReleasedThisFrame())
            movement.StopJump();

        // Visual feedback
        //animator.SetBool("grounded", movement.IsGrounded);
        //animator.SetFloat("velocityX", Mathf.Abs(movement.CurrentVelocity.x) / movement.maxSpeed);
        sprite.flipX = movement.CurrentVelocity.x < -0.01f;
    }

    protected override void OnDeath()
    {
        controlEnabled = false;
        base.OnDeath();
    }

    private void OnEnable()
    {
        moveAction?.Enable();
        jumpAction?.Enable();
    }

    private void OnDisable()
    {
        moveAction?.Disable();
        jumpAction?.Disable();
    }
}