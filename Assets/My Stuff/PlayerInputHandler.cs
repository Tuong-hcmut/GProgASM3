using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Handles all player input and forwards it to subsystems:
/// - Movement input forwarded to PlayerMovement (SetMoveInput/TryJump/StopJump)
/// - Attack input forwarded to PlayerAttack (TryAttack) with vertical direction hint
/// 
/// Note:
/// - This class must not contain gameplay logic (movement physics, damage). It only reads input
///   and calls into other components that implement behavior.
/// - Move the sprite flipping to somewhere more appropriate later.
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(PlayerAttack))]
public class PlayerInputHandler : MonoBehaviour
{
    private InputAction moveAction;
    private InputAction jumpAction;
    private InputAction attackAction;

    private SpriteRenderer spriteRenderer;
    private Animator animator;
    private PlayerAttack playerAttack;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
        playerAttack = GetComponent<PlayerAttack>();

        var map = InputSystem.actions;
        moveAction = map.FindAction("Player/Move");
        jumpAction = map.FindAction("Player/Jump");
        attackAction = map.FindAction("Player/Attack");

        OnEnable();
    }

    /// <summary>
    /// Read and forward input to the provided PlayerMovement instance.
    /// Keep input collection and delegation here; do not change movement state directly.
    /// </summary>
    public void HandleInput(PlayerMovement movement)
    {
        // --- Movement ---
        Vector2 input = moveAction.ReadValue<Vector2>();
        movement.SetMoveInput(input.x);

        if (jumpAction.WasPressedThisFrame())
            movement.TryJump();

        if (jumpAction.WasReleasedThisFrame())
            movement.StopJump();

        // --- Attack ---
        if (attackAction.WasPressedThisFrame())
        {
            // pass vertical direction so attack decides up/forward/down
            playerAttack.TryAttack(input.y);
        }

        // --- Visual feedback ---
        spriteRenderer.flipX = movement.CurrentVelocity.x < -0.01f;

        // animator.SetBool("grounded", movement.IsGrounded);
        // animator.SetFloat("velocityX", Mathf.Abs(movement.CurrentVelocity.x) / movement.maxSpeed);
    }

    private void OnEnable()
    {
        moveAction?.Enable();
        jumpAction?.Enable();
        attackAction?.Enable();
    }

    private void OnDisable()
    {
        moveAction?.Disable();
        jumpAction?.Disable();
        attackAction?.Disable();
    }
}