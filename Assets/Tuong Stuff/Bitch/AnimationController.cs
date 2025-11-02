/// <summary>
/// AnimationController — A lightweight collection of animator "presets" and helper methods.
///
/// Purpose:
/// - Expose named, single-responsibility methods that wrap Animator calls (SetInteger, SetBool, SetTrigger, ResetTrigger, Play, etc.).
/// - Hold Animator reference(s) and all Animator parameter name hashes/constants in one place to avoid magic strings across the codebase.
/// - Provide a stable API for other systems (PlayerMovement, PlayerController, PlayerAttack) to request animations without performing Animator calls themselves.
///
/// Constraints:
/// - This class does NOT contain gameplay logic or decisions about when animations should play.
/// - It simply executes the animation commands passed by controllers and ensures parameter names/types are centralized and validated.
/// Note to self:
///  - Research statemachines (again), specifically how to handle state transitions
/// cleanly for animation syncing and control flow.
///  - This version’s close to final — minimal future refactoring expected.
/// </summary>
using UnityEngine;

public class AnimationController : MonoBehaviour
{
    Animator animator;
    public int groundedBool { get; private set; }
    public int slidingBool { get; private set; }
    public int movementSpeed { get; private set; }
    public int velocitySpeed { get; private set; }
    public int jumpTrigger { get; private set; }
    public int doubleJumpTrigger { get; private set; }
    public int slideJumpTrigger { get; private set; }
    public int turnTrigger { get; private set; }
    public int respawnTrigger { get; private set; }

    void Awake()
    {
        animator = GetComponent<Animator>();
        groundedBool = Animator.StringToHash("Grounded");
        slidingBool = Animator.StringToHash("Sliding");
        movementSpeed = Animator.StringToHash("Movement");
        velocitySpeed = Animator.StringToHash("Velocity");
        jumpTrigger = Animator.StringToHash("Jump");
        doubleJumpTrigger = Animator.StringToHash("DoubleJump");
        slideJumpTrigger = Animator.StringToHash("SlideJump");
        turnTrigger = Animator.StringToHash("Turn");
        respawnTrigger = Animator.StringToHash("Respawn");
    }

    public void SetBool(int hash, bool val) => animator.SetBool(hash, val);
    public void SetFloat(int hash, float val) => animator.SetFloat(hash, val);
    public void SetInteger(int hash, int val) => animator.SetInteger(hash, val);
    public void SetTrigger(int hash) => animator.SetTrigger(hash);
    public void ResetTrigger(int hash) => animator.ResetTrigger(hash);
    public void Play(string clip) => animator.Play(clip);
    public T GetBehaviour<T>() where T : StateMachineBehaviour => animator.GetBehaviour<T>();
    public Animator GetAnimator() => animator;
}