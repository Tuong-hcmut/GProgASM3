using UnityEngine;

/// <summary>
/// PlayerController — Handles communication between input, movement and entity lifecycle.
/// Responsibilities:
///  - Read input from PlayerInputHandler and forward it to PlayerMovement (via HandleInput)
///  - Manage control gating (e.g. disable controls on death)
/// 
/// Note: PlayerController must not contain any gameplay logic and only act as a coordinator.
/// </summary>
[RequireComponent(typeof(PlayerMovement))]
[RequireComponent(typeof(PlayerInputHandler))]
//[RequireComponent(typeof(AudioSource))]
public class PlayerController : BaseEntity
{
    [Header("Player Settings")]
    [SerializeField] private bool controlEnabled = true;

    private PlayerInputHandler inputHandler;
    private PlayerMovement movement;

    protected override void Awake()
    {
        base.Awake();
        inputHandler = GetComponent<PlayerInputHandler>();
        movement = GetComponent<PlayerMovement>();
    }

    protected override void Update()
    {
        if (!controlEnabled || !IsAlive) return;
        inputHandler.HandleInput(movement);
    }

    protected override void OnDeath()
    {
        controlEnabled = false;
        base.OnDeath();
    }
}