using UnityEngine;

/// <summary>
/// Handles movement physics for the player (ground check, jumping, velocity).
/// </summary>
/// note to self, maybe consider using kinematic body instead
[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    public float maxSpeed = 7f;
    public float jumpForce = 7f;
    public float jumpCutMultiplier = 0.5f;

    [Header("Ground Check")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float groundRadius = 0.1f;

    public bool IsGrounded { get; private set; }
    public Vector2 CurrentVelocity => rb.linearVelocity;

    private Rigidbody2D rb;
    private float moveInput;
    private bool jumpQueued;
    private bool stopJumpQueued;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        IsGrounded = Physics2D.OverlapCircle(groundCheck.position, groundRadius, groundLayer);
    }

    void FixedUpdate()
    {
        // Horizontal
        rb.linearVelocity = new Vector2(moveInput * maxSpeed, rb.linearVelocity.y);

        // Jump start
        if (jumpQueued && IsGrounded)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            jumpQueued = false;
        }

        // Early jump release
        if (stopJumpQueued)
        {
            stopJumpQueued = false;
            if (rb.linearVelocity.y > 0)
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.y * jumpCutMultiplier);
        }
    }

    // --- Public interface ---
    public void SetMoveInput(float input) => moveInput = input;
    public void TryJump() => jumpQueued = true;
    public void StopJump() => stopJumpQueued = true;
}
