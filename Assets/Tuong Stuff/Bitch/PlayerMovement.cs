/// <summary>
/// Handles kinematic-style movement physics for the player:
/// - Manual gravity & velocity integration
/// - Raycast/box-cast based ground & wall detection
/// - Jump buffering, multi-jump (configurable), early jump cut
/// - Centralized application of all external impulses/recoil (attacks, hurt, death, dash, sprint)
/// - Also handles relevant animation and state changes
/// IMPORTANT FUCKING NOTES:
/// - God, i fucking hate myself for selecting kinematic movement for the player.
/// - This component is the ONLY code that may modify the transform/velocity of the player.
/// - External systems (attacks, BaseEntity/hurt pipeline, AI) must request movement changes
///   via the public API: ApplyExternalImpulse, ApplyAttackRecoil, StartSprint, SetInputEnabled, etc.
/// - Determination of recoil direction/flip logic is inside this component so facing semantics
///   are applied consistently in one place.
/// </summary>
using System.Collections;
//using System.Numerics;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    #region Variables
    [Header("Movement")]
    [SerializeField] float maxSpeed = 0.0f;
    [SerializeField] float jumpForce = 0.0f;
    [SerializeField] float wallJumpForce = 0.0f;
    [SerializeField] float wallReactingForce = 0.0f;
    [SerializeField] float recoilForce = 0.0f;
    [SerializeField] float downRecoilForce = 0.0f;
    [SerializeField] float hurtForce = 0.0f;
    [SerializeField] float maxGravityVelocity = 10.0f;
    [SerializeField] float jumpGravityScale = 1.0f;
    [SerializeField] float fallGravityScale = 1.0f;
    [SerializeField] float slidingGravityScale = 1.0f;
    [SerializeField] float groundedGravityScale = 1.0f;

    private Rigidbody2D rb;
    PlayerInputHandler inputHandler;
    AnimationController anim;

    private Vector2 vectorInput;
    private bool jumpInput = false;
    private bool enableGravity = false;
    private int jumpCount;

    private bool isOnGround = true;
    private bool isFacingLeft = false;
    private bool isJumping = false;
    private bool isSliding = false;
    private bool isFalling;

    public bool canMove { get; set; } = true;

    readonly Quaternion flippedScale = Quaternion.Euler(0, 180, 0);
    readonly Quaternion normalScale = Quaternion.Euler(0, 0, 0);

    private BaseEntity baseEntity;
    #endregion
    #region RunJump
    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        inputHandler = GetComponent<PlayerInputHandler>();
        anim = GetComponent<AnimationController>();
        baseEntity = GetComponent<BaseEntity>();
        if (inputHandler != null)
        {
            inputHandler.JumpStarted += OnJumpStarted;
            inputHandler.JumpPerformed += ctx => OnJumpPerformed();
            inputHandler.JumpCanceled += ctx => OnJumpPerformed();
        }
        enableGravity = true;
    }

    void FixedUpdate()
    {
        // read input
        if (inputHandler != null) vectorInput = inputHandler.MoveInput;
        UpdateVelocity();
        UpdateDirection();
        UpdateJump();
        UpdateGravityScale();
        if (anim != null)
            anim.SetFloat(anim.velocitySpeed, rb.linearVelocity.y);
    }

    private void UpdateVelocity()
    {
        if (!baseEntity.GetIsDead())
        {
            Vector2 velocity = rb.linearVelocity;
            if (isSliding && vectorInput.x != 0)
            {
                velocity.y = Mathf.Clamp(velocity.y, -maxGravityVelocity / 2, maxGravityVelocity / 2);
            }
            else
            {
                velocity.y = Mathf.Clamp(velocity.y, -maxGravityVelocity, maxGravityVelocity);
            }

            if (canMove && baseEntity.gameManager.IsEnableInput())
            {
                rb.linearVelocity = new Vector2(vectorInput.x * maxSpeed, velocity.y);
                if (anim != null) anim.SetInteger(anim.movementSpeed, (int)vectorInput.x);
            }
        }
        else
        {
            Vector2 velocity = rb.linearVelocity;
            velocity.x = 0;
            velocity.y = Mathf.Clamp(velocity.y, -maxGravityVelocity, maxGravityVelocity);
            rb.linearVelocity = velocity;
        }
    }

    private void OnJumpStarted(UnityEngine.InputSystem.InputAction.CallbackContext context)
    {
        OnJumpStarted();
    }

    private void OnJumpStarted()
    {
        if (baseEntity.GetIsDead()) return;
        if (isSliding && !isOnGround)
        {
            StartCoroutine(GrabWallJump());
            return;
        }

        if (!baseEntity.gameManager.IsEnableInput()) return;

        if (jumpCount <= 1)
        {
            ++jumpCount;
            if (jumpCount == 1)
            {
                if (anim != null) anim.Play("Jump");
                //if (anim != null) anim.SetTrigger(anim.jumpTrigger);
                baseEntity.audioEffectPlayer?.Play(PlayerAudio.AudioType.Jump, true);
            }
            else if (jumpCount == 2)
            {
                if (anim != null) anim.Play("Jump");
                //if (anim != null) anim.SetTrigger(anim.doubleJumpTrigger);
                // effecter?.DoEffect(CharacterEffect.EffectType.DoubleJump, true);
                baseEntity.audioEffectPlayer?.Play(PlayerAudio.AudioType.HeroWings, true);
            }
            else
            {
                return;
            }
            jumpInput = true;
        }
    }

    private void OnJumpPerformed()
    {
        jumpInput = false;
        isJumping = false;
        /*
        if (jumpCount == 1 && anim != null)
            anim.ResetTrigger(anim.jumpTrigger);
        else if (jumpCount == 2 && anim != null)
            anim.ResetTrigger(anim.doubleJumpTrigger);*/
        // Didn't decide to do separate double jump animations in the end, 
        // ill leave this in just in case i change my mind
    }

    private void UpdateJump()
    {
        if (isJumping && rb.linearVelocity.y < 0) isFalling = true;

        if (jumpInput && baseEntity.gameManager.IsEnableInput())
        {
            rb.AddForce(new Vector2(0, jumpForce), ForceMode2D.Impulse);
            isJumping = true;
            //   effecter?.DoEffect(CharacterEffect.EffectType.FallTrail, false);
        }

        if (isOnGround && !isJumping && jumpCount != 0)
        {
            jumpCount = 0;
        }
    }

    private void UpdateDirection()
    {
        if (canMove && !baseEntity.GetIsDead())
        {
            if (rb.linearVelocity.x > 0 && isFacingLeft)
            {
                //print("flip to right");
                isFacingLeft = false;
                transform.rotation = normalScale;
                //anim.transform.localScale = flippedScale;
            }
            else if (rb.linearVelocity.x < 0 && !isFacingLeft)
            {
                //print("flip to left");
                isFacingLeft = true;
                transform.rotation = flippedScale;
                //anim.transform.localScale = Vector3.one;
            }
        }
    }

    private void UpdateGravityScale()
    {
        var gravityScale = groundedGravityScale;

        if (!isOnGround)
        {
            if (isSliding && vectorInput.x != 0)
                gravityScale = slidingGravityScale;
            else
                gravityScale = rb.linearVelocity.y > 0.0f ? jumpGravityScale : fallGravityScale;
        }

        if (!enableGravity) gravityScale = 0;
        rb.gravityScale = gravityScale;
    }

    IEnumerator GrabWallJump()
    {
        baseEntity.gameManager.SetEnableInput(false);
        enableGravity = false;
        if (anim != null) anim.Play("Jump");
        int direction = isFacingLeft ? 1 : -1;
        rb.linearVelocity = new Vector2(transform.lossyScale.x * wallReactingForce * direction, wallJumpForce);
        yield return new WaitForSeconds(0.15f);
        enableGravity = true;
        baseEntity.gameManager.SetEnableInput(true);
    }

    public void StopHorizontalMovement()
    {
        Vector2 velocity = rb.linearVelocity;
        velocity.x = 0;
        rb.linearVelocity = velocity;
        if (anim != null) anim.SetInteger(anim.movementSpeed, 0);
    }
    #endregion
    public void StopInput()
    {
        baseEntity.gameManager.SetEnableInput(false);
        StopHorizontalMovement();
    }

    public void ResumeInput()
    {
        baseEntity.gameManager.SetEnableInput(true);
    }
    #region Hurt
    public void AddDownRecoilForce()
    {
        var vel = rb.linearVelocity;
        vel.y = 0;
        rb.linearVelocity = vel;
        rb.AddForce(Vector2.up * downRecoilForce, ForceMode2D.Force);
    }

    public IEnumerator AddRecoilForce()
    {
        canMove = false;
        if (transform.localScale.x < 0)
            rb.AddForce(Vector2.right * recoilForce, ForceMode2D.Force);
        else
            rb.AddForce(Vector2.left * recoilForce, ForceMode2D.Force);
        yield return new WaitForSeconds(0.2f);
        canMove = true;
    }

    // Apply hurt impulse/velocity
    public void ApplyDamageMovement()
    {
        if (transform.localScale.x < 0)
            rb.linearVelocity = new Vector2(-1f, 1f) * hurtForce;
        else
            rb.linearVelocity = new Vector2(1f, 1f) * hurtForce;
    }
    #endregion
    #region Collision/Grounding
    private void OnCollisionEnter2D(Collision2D collision) => UpdateGrounding(collision, false);
    private void OnCollisionStay2D(Collision2D collision) => UpdateGrounding(collision, false);
    private void OnCollisionExit2D(Collision2D collision) => UpdateGrounding(collision, true);

    private void UpdateGrounding(Collision2D collision, bool exitState)
    {
        if (exitState)
        {
            if ((collision.gameObject.layer == LayerMask.NameToLayer("Ground") || collision.gameObject.layer == LayerMask.NameToLayer("Soft Terrain")))
            {
                isOnGround = false;
                isSliding = false;
            }
        }
        else
        {
            if ((collision.gameObject.layer == LayerMask.NameToLayer("Ground") || collision.gameObject.layer == LayerMask.NameToLayer("Soft Terrain"))
                && collision.contacts[0].normal == Vector2.up && !isOnGround)
            {
                isOnGround = true;
                isJumping = false;
                isFalling = false;
                //  effecter?.DoEffect(CharacterEffect.EffectType.FallTrail, true);
            }
            else if ((collision.gameObject.layer == LayerMask.NameToLayer("Ground") || collision.gameObject.layer == LayerMask.NameToLayer("Soft Terrain"))
                && collision.contacts[0].normal == Vector2.down && isJumping)
            {
                OnJumpPerformed();
            }
        }

        if (anim != null) anim.SetBool(anim.groundedBool, isOnGround);
    }
    #endregion

    // small helpers for other systems
    public void SetIsSliding(bool state)
    {
        isSliding = state;
        if (!baseEntity.GetIsDead() && anim != null) anim.SetBool(anim.slidingBool, isSliding);
    }

    public void SetIsOnGrounded(bool state)
    {
        isOnGround = state;
        if (!baseEntity.GetIsDead() && anim != null) anim.SetBool(anim.groundedBool, isOnGround);
    }

    public void ResetFallDistance()
    {
        if (anim != null) anim.GetBehaviour<FallingBehaviour>().ResetAllParams();
    }

    public void SlideWall_ResetJumpCount()
    {
        jumpCount = 1;
    }

    public bool GetIsOnGround() => isOnGround;
}