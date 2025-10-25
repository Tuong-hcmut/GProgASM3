using UnityEngine;

/// <summary>
/// Handles kinematic-style movement physics for the player:
/// - Manual gravity & velocity integration
/// - Raycast-based ground detection
/// - Jumping, early release, and horizontal motion
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    public float maxSpeed = 7f;
    public float acceleration = 40f;
    public float jumpForce = 14f;
    public float gravity = 30f;
    public float jumpCutMultiplier = 0.5f;
    [SerializeField] private float jumpBufferTime = 0.1f;  // how long the jump input lasts
    private float jumpBufferCounter = 0f;


    [Header("Ground Detection")]
    //[SerializeField] private Transform footObject1;
    //[SerializeField] private Transform footObject2;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float groundCheckDistance = 0.1f;

    // === [Runtime State — conventional] ===
    public bool IsGrounded { get; private set; }
    public Vector2 CurrentVelocity => velocity;

    private Vector2 velocity;
    private float moveInput;
    private bool jumpQueued;
    private bool stopJumpQueued;
    private Collider2D col;

    void Awake()
    {
        col = GetComponent<Collider2D>();

        // Disable Rigidbody if present
        var rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.isKinematic = true;
            rb.useFullKinematicContacts = false;
            rb.simulated = false;
        }
    }

    void Update()
    {
        GroundCheck();
        HandleJump();
        ApplyGravity();
        ApplyHorizontal();
        MoveCharacter();
    }

    // === [Movement internals] ===
    private void GroundCheck()
    {
        /*bool leftHit = Physics2D.Raycast(footObject1.position, Vector2.down, groundCheckDistance, groundLayer);
        bool rightHit = Physics2D.Raycast(footObject2.position, Vector2.down, groundCheckDistance, groundLayer);
        IsGrounded = leftHit || rightHit;*/
        Bounds b = col.bounds;
        Vector2 boxSize = b.size * new Vector2(0.95f, 0.5f);
        Vector2 boxCenter = (Vector2)b.center + Vector2.down * (b.extents.y - boxSize.y * 0.5f + 0.02f);
        IsGrounded = Physics2D.BoxCast(
            boxCenter,
            boxSize,
            0f,
            Vector2.down,
            0.02f,
            groundLayer
        );
    }

    private void ApplyGravity()
    {
        if (!IsGrounded)
            velocity.y -= gravity * Time.deltaTime;
        else if (velocity.y < 0)
            velocity.y = 0f;// small stick force to stay grounded
    }

    private void HandleJump()
    {
        if (jumpBufferCounter > 0)
            jumpBufferCounter -= Time.deltaTime;

        // If jump queued externally, refresh the buffer timer
        if (jumpQueued)
        {
            jumpBufferCounter = jumpBufferTime;
            jumpQueued = false;
        }

        // If grounded and jump buffer still active -> jump
        if (jumpBufferCounter > 0 && IsGrounded)
        {
            velocity.y = jumpForce;
            IsGrounded = false;
            jumpBufferCounter = 0f;
        }

        // Handle early release
        if (stopJumpQueued)
        {
            stopJumpQueued = false;
            if (velocity.y > 0)
                velocity.y *= jumpCutMultiplier;
        }
    }

    private void ApplyHorizontal()
    {
        float targetSpeed = moveInput * maxSpeed;
        velocity.x = Mathf.MoveTowards(velocity.x, targetSpeed, acceleration * Time.deltaTime);
    }

    private void MoveCharacter()
    {
        //transform.Translate(velocity * Time.deltaTime);
        Vector2 move = velocity * Time.deltaTime;

        // --- Vertical collision check ---
        if (move.y != 0)
        {
            RaycastHit2D hitY = Physics2D.BoxCast(
                col.bounds.center,
                col.bounds.size,
                0f,
                Vector2.up * Mathf.Sign(move.y),
                Mathf.Abs(move.y),
                groundLayer
            );

            if (hitY)
            {
                float distance = Mathf.Max(hitY.distance - 0.01f, 0f);
                move.y = Mathf.Sign(move.y) * distance;
                velocity.y = 0;
            }
        }

        // --- Horizontal collision check (optional) ---
        if (move.x != 0)
        {
            RaycastHit2D hitX = Physics2D.BoxCast(
                col.bounds.center,
                col.bounds.size,
                0f,
                Vector2.right * Mathf.Sign(move.x),
                Mathf.Abs(move.x),
                groundLayer
            );

            if (hitX && Mathf.Abs(hitX.normal.y) < 0.5f)
            {
                float distance = hitX.distance - 0.01f;
                move.x = Mathf.Sign(move.x) * distance;
                velocity.x = 0;
            }
        }

        transform.Translate(move);
    }

    // === [Public Interface — for PlayerController] ===
    public void SetMoveInput(float input) => moveInput = input;
    public void TryJump() => jumpQueued = true;
    public void StopJump() => stopJumpQueued = true;

    /*// === [Debug Visualization] ===
    private void OnDrawGizmosSelected()
    {
        if (footObject1 == null || footObject2 == null) return;

        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(footObject1.position, footObject1.position + Vector3.down * groundCheckDistance);
        Gizmos.DrawLine(footObject2.position, footObject2.position + Vector3.down * groundCheckDistance);
    }*/
}
