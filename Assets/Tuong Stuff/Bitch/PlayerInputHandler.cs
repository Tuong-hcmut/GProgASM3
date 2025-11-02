/// <summary>
/// Handles all player input and forwards it to subsystems via PlayerController:
/// - Movement input forwarded to PlayerMovement (SetMoveInput/TryJump/StopJump)
/// - Attack input forwarded to PlayerAttack (TryAttack) with vertical direction hint
/// 
/// Note:
/// - This class must not contain gameplay logic (movement physics, damage). It only reads input
///   and calls into other components that implement behavior.
/// - Move the sprite flipping to somewhere more appropriate later.
/// </summary>
using UnityEngine;
using UnityEngine.InputSystem;
using System;

public class PlayerInputHandler : MonoBehaviour
{
    public Vector2 MoveInput { get; private set; }
    public bool JumpHeld { get; private set; }

    public event Action<InputAction.CallbackContext> JumpStarted;
    public event Action<InputAction.CallbackContext> JumpPerformed;
    public event Action<InputAction.CallbackContext> JumpCanceled;
    public event Action<InputAction.CallbackContext> AttackStarted;
    public event Action<InputAction.CallbackContext> AttackPerformed;
    public event Action<InputAction.CallbackContext> AttackCanceled;

    private void OnEnable()
    {
        var autogen = InputManager.InputControl.GamePlayer;
        autogen.Movement.performed += ctx => MoveInput = ctx.ReadValue<Vector2>();
        autogen.Movement.canceled += ctx => MoveInput = Vector2.zero;

        autogen.Jump.started += ctx => JumpStarted?.Invoke(ctx);
        autogen.Jump.performed += ctx => JumpPerformed?.Invoke(ctx);
        autogen.Jump.canceled += ctx => JumpCanceled?.Invoke(ctx);

        autogen.Attack.started += ctx => AttackStarted?.Invoke(ctx);
        autogen.Attack.performed += ctx => AttackPerformed?.Invoke(ctx);
        autogen.Attack.canceled += ctx => AttackCanceled?.Invoke(ctx);
    }

    private void OnDisable()
    {
        var autogen = InputManager.InputControl.GamePlayer;
        autogen.Movement.performed -= ctx => MoveInput = ctx.ReadValue<Vector2>();
        autogen.Movement.canceled -= ctx => MoveInput = Vector2.zero;

        autogen.Jump.started -= ctx => JumpStarted?.Invoke(ctx);
        autogen.Jump.performed -= ctx => JumpPerformed?.Invoke(ctx);
        autogen.Jump.canceled -= ctx => JumpCanceled?.Invoke(ctx);

        autogen.Attack.started -= ctx => AttackStarted?.Invoke(ctx);
        autogen.Attack.performed -= ctx => AttackPerformed?.Invoke(ctx);
        autogen.Attack.canceled -= ctx => AttackCanceled?.Invoke(ctx);
    }
}