using System.Collections;
using UnityEngine;

/// <summary>
/// Handles kinematic-style movement physics for the player:
/// - Manual gravity & velocity integration
/// - Raycast/box-cast based ground & wall detection
/// - Jump buffering, multi-jump (configurable), early jump cut
/// - Centralized application of all external impulses/recoil (attacks, hurt, death, dash, sprint)
/// 
/// IMPORTANT FUCKING NOTES:
/// - God, i fucking hate myself for selecting kinematic movement for the player.
/// - This component is the ONLY code that may modify the transform/velocity of the player.
/// - External systems (attacks, BaseEntity/hurt pipeline, AI) must request movement changes
///   via the public API: ApplyExternalImpulse, ApplyAttackRecoil, StartSprint, SetInputEnabled, etc.
/// - Determination of recoil direction/flip logic is inside this component so facing semantics
///   are applied consistently in one place.
/// </summary>
[RequireComponent(typeof(Collider2D))]
[DisallowMultipleComponent]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    public float maxSpeed = 70f;          // initial horizontal speed
    public float jumpForce = 140f;        // upward velocity when jumping
    public float gravity = 300f;          // gravity applied when airborne
    public float jumpCutMultiplier = 0.5f; // early release multiplier

    [Header("Jump Buffer")]
    [SerializeField] private float jumpBufferTime = 0.1f;
    private float jumpBufferCounter = 0f;

    [Header("Coyote / Grounding")]
    [Tooltip("Grace period after leaving ground where a jump is still allowed.")]
    [SerializeField] private float coyoteTime = 0.08f;
    private float coyoteTimer = 0f;

    [Header("Jump Settings")]
    [Tooltip("Maximum number of jumps (1 = single jump, 2 = double jump).")]
    [SerializeField] private int maxJumps = 2;
    private int jumpsRemaining;
    private bool prevGrounded = false;

    [Header("Ground Detection")]
    [SerializeField] private LayerMask groundLayer = 6;
    [SerializeField] private float groundCastInsetY = 0.02f;
    [SerializeField] private float groundCastExtraDistance = 0.02f;

    [Header("Slope Handling")]
    [Tooltip("Maximum angle (degrees) considered walkable ground. Surfaces steeper than this become 'steep' and will not count as grounded.")]
    [SerializeField] private float maxGroundAngle = 50f;
    [Tooltip("Multiplier applied to gravity along steep slopes to make player slide down.")]
    [SerializeField] private float slopeSlideFriction = 1.0f;

    // runtime slope state
    private bool onSteepSlope = false;
    private Vector2 slopeNormal = Vector2.up;
    public bool OnSteepSlope => onSteepSlope;
    public Vector2 SlopeNormal => slopeNormal;

    [Header("Recoil Settings (attacks/hurt/death)")]
    [Tooltip("Recoil applied to player when performing an upward attack or hit.")]
    [SerializeField] private Vector2 attackUpRecoil = Vector2.zero;
    [Tooltip("Recoil applied to player when performing a forward attack or hit. X will be flipped by facing.")]
    [SerializeField] private Vector2 attackForwardRecoil = Vector2.zero;
    [Tooltip("Recoil applied to player when performing a downward attack or hit.")]
    [SerializeField] private Vector2 attackDownRecoil = Vector2.zero;

    [Header("Wall / Climb")]
    [Tooltip("Velocity applied horizontally (x) and vertically (y) when performing a wall/climb jump.")]
    [SerializeField] private Vector2 climbJumpForce = new Vector2(140f, 140f);
    [Tooltip("Delay during which input is disabled after a climb-jump (keeps behavior consistent with animator).")]
    [SerializeField] private float climbJumpDelay = 0.2f;

    // Runtime state
    public bool IsGrounded { get; private set; }
    public bool IsClimb { get; private set; }
    private Rigidbody2D rb;
    public Vector2 CurrentVelocity => rb != null ? rb.linearVelocity : velocity;

    private Vector2 velocity;
    private float moveInput;
    private bool jumpQueued;
    private bool stopJumpQueued;
    private Collider2D hitbox;

    // Input / sprint / control state
    private bool inputEnabled = true;
    private bool isSprinting = false;

    void Awake()
    {
        hitbox = GetComponent<Collider2D>();
        jumpsRemaining = maxJumps;
        prevGrounded = false;

        // Ensure there is a Rigidbody2D we can drive via velocity
        rb = GetComponent<Rigidbody2D>();
        if (rb == null) rb = gameObject.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.freezeRotation = true;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        // disable engine gravity because we apply manual gravity in ApplyGravity()
        rb.gravityScale = 0f;
        rb.simulated = true;
    }

    void Update()
    {
        // Physics-style update (kinematic)
        GroundCheck();
        // landing detection: reset jumps on landing and exit climb state
        if (IsGrounded && !prevGrounded)
        {
            jumpsRemaining = maxJumps;
            IsClimb = false;
        }
        prevGrounded = IsGrounded;

        HandleJumpBuffer();
        ApplyGravity();
        ApplyHorizontalImmediate();
        MoveCharacter();
    }

    // ----- Core movement internals -----

    private void GroundCheck()
    {
        // Perform a short, localized circle cast from the feet area downwards and require
        // a sufficiently-upward normal so walls/side contacts don't register as ground.
        Bounds b = hitbox.bounds;

        // origin at just below collider center (feet area) — start a little outside the collider
        Vector2 origin = (Vector2)b.center + Vector2.down * (b.extents.y + groundCastInsetY + 0.01f);

        // small radius roughly covering player's feet (avoid touching walls)
        float radius = Mathf.Clamp(b.size.x * 0.35f, 0.05f, 0.25f);

        // avoid casting that starts inside our own collider
        bool prevQueries = Physics2D.queriesStartInColliders;
        Physics2D.queriesStartInColliders = false;
        RaycastHit2D[] hits = Physics2D.CircleCastAll(origin, radius, Vector2.down, groundCastExtraDistance, groundLayer);
        Physics2D.queriesStartInColliders = prevQueries;

        IsGrounded = false;
        onSteepSlope = false;
        slopeNormal = Vector2.up;

        // compute min required normal.y from configured max ground angle
        float minGroundNormalY = Mathf.Cos(maxGroundAngle * Mathf.Deg2Rad);
        for (int i = 0; i < hits.Length; i++)
        {
            var h = hits[i];
            if (h.collider == null) continue;
            // require a mostly-upward normal to be considered ground (filter out walls)
            if (h.normal.y >= minGroundNormalY)
            {
                IsGrounded = true;
                break;
            }
            // if the surface points upwards but is steeper than maxGroundAngle, mark as steep
            if (h.normal.y > 0f && h.normal.y < minGroundNormalY)
            {
                onSteepSlope = true;
                slopeNormal = h.normal;
                // continue scanning; a flatter contact may still count as ground
            }
        }
    }

    private void HandleJumpBuffer()
    {
        if (jumpBufferCounter > 0f)
            jumpBufferCounter -= Time.deltaTime;

        if (jumpQueued)
        {
            // external input requested a jump — refresh buffer
            jumpBufferCounter = jumpBufferTime;
            jumpQueued = false;
        }

        // If grounded (or within coyote) and buffer active → perform immediate jump
        if (jumpBufferCounter > 0f && (IsGrounded || coyoteTimer > 0f) && inputEnabled)
        {
            // only perform jump if we have jumps available (respects multi-jump)
            if (jumpsRemaining > 0)
            {
                velocity.y = jumpForce;
                IsGrounded = false;
                jumpsRemaining--;
                jumpBufferCounter = 0f;
            }
        }

        // Handle early release (cut jump)
        if (stopJumpQueued)
        {
            stopJumpQueued = false;
            if (velocity.y > 0f)
                velocity.y *= jumpCutMultiplier;
        }
    }

    private void ApplyGravity()
    {
        if (!IsGrounded && !IsClimb)
        {
            velocity.y -= gravity * Time.deltaTime;
        }
        else if (IsGrounded && velocity.y < 0f)
        {
            velocity.y = 0f; // small stick to ground
        }
    }

    private void ApplyHorizontalImmediate()
    {
        if (isSprinting)
        {
            // sprint coroutine controls velocity.x directly while sprinting
            return;
        }

        // snappy immediate movement: no smoothing
        if (inputEnabled)
            velocity.x = moveInput * maxSpeed;
        else
            velocity.x = 0f;
    }

    private void MoveCharacter()
    {
        Vector2 move = velocity * Time.deltaTime;
        float minGroundNormalY = Mathf.Cos(maxGroundAngle * Mathf.Deg2Rad);
        float originalMoveY = move.y;

        // Vertical collision check
        if (move.y != 0f)
        {
            bool prevQueries = Physics2D.queriesStartInColliders;
            Physics2D.queriesStartInColliders = false;
            RaycastHit2D hitY = Physics2D.BoxCast(
                hitbox.bounds.center,
                hitbox.bounds.size,
                0f,
                Vector2.up * Mathf.Sign(move.y),
                Mathf.Abs(move.y),
                groundLayer
            );
            Physics2D.queriesStartInColliders = prevQueries;

            if (hitY)
            {
                float distance = Mathf.Max(hitY.distance - 0.01f, 0f);
                move.y = Mathf.Sign(move.y) * distance;

                // If we hit something while moving downward and the surface normal is floor-like,
                // treat this as a landing: zero vertical velocity, mark grounded and reset jumps.
                if (originalMoveY < 0f && hitY.normal.y >= minGroundNormalY)
                {
                    velocity.y = 0f;
                    IsGrounded = true;
                    jumpsRemaining = maxJumps;
                    IsClimb = false;
                }
                // hit a steep slope (upward-facing but too steep)
                else if (originalMoveY < 0f && hitY.normal.y > 0f && hitY.normal.y < minGroundNormalY)
                {
                    // do not count as grounded, but apply sliding down the slope
                    IsGrounded = false;
                    onSteepSlope = true;
                    slopeNormal = hitY.normal;
                    // downhill direction along slope
                    Vector2 slopeDown = new Vector2(-hitY.normal.x, -hitY.normal.y).normalized;
                    // accelerate down slope (gravity projection multiplied by friction)
                    velocity += slopeDown * gravity * slopeSlideFriction * Time.deltaTime;
                    // clamp a bit to avoid sticking into slope
                    if (velocity.y > 0f) velocity.y = 0f;
                }
                else
                {
                    // ceiling or other vertical stop: just cancel vertical velocity
                    velocity.y = 0f;
                }
            }
        }

        // Horizontal collision check (prevent passing through walls)
        if (move.x != 0f)
        {
            RaycastHit2D hitX = Physics2D.BoxCast(
                hitbox.bounds.center,
                hitbox.bounds.size,
                0f,
                Vector2.right * Mathf.Sign(move.x),
                Mathf.Abs(move.x),
                groundLayer
            );

            if (hitX && Mathf.Abs(hitX.normal.y) < 0.5f)
            {
                float distance = Mathf.Max(hitX.distance - 0.01f, 0f);
                move.x = Mathf.Sign(move.x) * distance;
                velocity.x = 0f;
            }
        }

        // Apply resulting velocity to Rigidbody2D so movement is done via physics engine
        if (rb != null)
        {
            rb.linearVelocity = velocity;
        }
        else
        {
            // fallback to previous behaviour if no Rigidbody present
            transform.Translate(move);
        }
    }

    // update coyote timer and prevGrounded after movement in Update()
    private void LateUpdate()
    {
        // maintain a short grace period after leaving ground
        if (IsGrounded)
            coyoteTimer = coyoteTime;
        else
            coyoteTimer = Mathf.Max(0f, coyoteTimer - Time.deltaTime);

        // clear steep slope state when not contacting slope
        if (!IsGrounded && !onSteepSlope)
        {
            slopeNormal = Vector2.up;
        }

        prevGrounded = IsGrounded;
    }

    // ----- Public API (for PlayerController / PlayerInputHandler / PlayerAttack) -----

    /// <summary>Set horizontal input [-1..1]. If input disabled, input is ignored.</summary>
    public void SetMoveInput(float input)
    {
        if (!inputEnabled) return;
        moveInput = Mathf.Clamp(input, -1f, 1f);
    }

    /// <summary>Queue a jump (uses the internal jump buffer).</summary>
    public void TryJump()
    {
        if (!inputEnabled) return;

        // If climbing, perform a wall/climb jump immediately
        if (IsClimb)
        {
            // perform immediate climb jump
            ClimbJump(climbJumpForce);
            // temporarily disable input to emulate previous behaviour and allow flip
            inputEnabled = false;
            StartCoroutine(ClimbJumpCoroutine(climbJumpDelay));
            // clear any queued jump state
            jumpQueued = false;
            jumpBufferCounter = 0f;
            return;
        }

        // Immediate ground jump (handles input order differences): if we're grounded jump immediately.
        if (IsGrounded)
        {
            if (jumpsRemaining > 0)
            {
                velocity.y = jumpForce;
                IsGrounded = false;
                jumpsRemaining--;
                jumpBufferCounter = 0f;
            }
            return;
        }

        // If airborne and have jumps left → perform immediate air jump (double jump)
        if (!IsGrounded && jumpsRemaining > 0)
        {
            velocity.y = jumpForce;
            jumpsRemaining--;
            return;
        }

        // otherwise, queue a jump (will execute when grounded within buffer window)
        jumpQueued = true;
        // activate buffer immediately so buffer logic is robust regardless of Update ordering
        jumpBufferCounter = jumpBufferTime;
    }

    /// <summary>Signal early jump release (will shorten upward velocity if applicable).</summary>
    public void StopJump()
    {
        stopJumpQueued = true;
    }

    /// <summary>Apply an immediate velocity override.</summary>
    public void ApplyExternalImpulse(Vector2 impulse)
    {
        // instant override like rigidbody.velocity = impulse
        velocity = impulse;
    }

    /// <summary>
    /// Determine and apply attack/hurt/death recoil based on vertical input direction
    /// (positive => up, negative => down, ~0 => forward) and current facing.
    /// 
    /// This centralizes recoil determination and mirroring rules inside PlayerMovement
    /// so external systems only indicate intent (verticalInput) and do not directly
    /// touch velocity/transform, ensuring relevant variables stay INSIDE this component.
    /// </summary>
    /// <param name="verticalInput">Direction hint: >0.5 up, < -0.5 down, otherwise forward.</param>
    public void ApplyAttackRecoil(float verticalInput)
    {
        Vector2 recoil;
        if (verticalInput > 0.5f)
        {
            recoil = attackUpRecoil;
        }
        else if (verticalInput < -0.5f)
        {
            recoil = attackDownRecoil;
        }
        else
        {
            // forward recoil is mirrored by facing (localScale.x)
            float facing = transform.localScale.x;
            Vector2 forward = attackForwardRecoil;
            forward.x = forward.x * -facing;
            recoil = forward;
        }

        ApplyExternalImpulse(recoil);
    }

    /// <summary>Enter climb state. Gravity is effectively removed; small downward slide applied.</summary>
    public void BeginClimb()
    {
        IsClimb = true;
        // imitate previous behavior: small slide down
        velocity.y = -2f;
        // after grabbing a wall, allow one jump (consistent with previous design)
        jumpsRemaining = 1;
    }

    /// <summary>Leave climb state and restore normal gravity.</summary>
    public void EndClimb()
    {
        IsClimb = false;
    }

    /// <summary>Perform a climb-jump impulse.</summary>
    public void ClimbJump(Vector2 climbJumpForce)
    {
        // apply as immediate velocity preserving kinematic style
        velocity.x = climbJumpForce.x * transform.localScale.x;
        velocity.y = climbJumpForce.y;
    }

    /// <summary>
    /// Start a sprint: temporarily overrides movement for sprintTime and blocks input for that duration.
    /// After sprintTime finishes, input is restored and sprintInterval delay is waited (no internal sprint reset tracking).
    /// </summary>
    public void StartSprint(float sprintSpeed, float sprintTime, float sprintInterval)
    {
        if (!inputEnabled || isSprinting) return;
        StartCoroutine(SprintCoroutine(sprintSpeed, sprintTime, sprintInterval));
    }

    private IEnumerator SprintCoroutine(float sprintSpeed, float sprintTime, float sprintInterval)
    {
        isSprinting = true;
        bool prevInput = inputEnabled;
        inputEnabled = false;

        // set sprint velocity relative to facing (keep same facing semantics)
        velocity.x = transform.localScale.x * sprintSpeed;
        velocity.y = 0f;

        // run sprint for sprintTime
        yield return new WaitForSeconds(sprintTime);

        // restore
        inputEnabled = prevInput;
        isSprinting = false;

        // optional interval pause (caller can manage reset flags like _isSprintReset)
        yield return new WaitForSeconds(sprintInterval);
    }

    /// <summary>Enable or disable movement input (used when hurt, dead, etc.).</summary>
    public void SetInputEnabled(bool enabled)
    {
        inputEnabled = enabled;
        if (!enabled)
        {
            moveInput = 0f;
        }
    }

    private IEnumerator ClimbJumpCoroutine(float delay)
    {
        yield return new WaitForSeconds(delay);
        // re-enable input
        inputEnabled = true;
        // flip facing so climb jump pushes away from wall
        Vector3 s = transform.localScale;
        s.x = -s.x;
        transform.localScale = s;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider != null && collision.collider.CompareTag("Wall") && !IsGrounded)
        {
            // enter climb state
            BeginClimb();
        }
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.collider != null && collision.collider.CompareTag("Wall") && velocity.y < 0f && !IsClimb)
        {
            // start climb if sliding along a wall while falling
            BeginClimb();
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.collider != null && collision.collider.CompareTag("Wall"))
        {
            EndClimb();
        }
    }
}