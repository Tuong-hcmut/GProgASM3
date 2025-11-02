/// <summary>
/// Handles player attack logic:
/// - Buffered attack input & cooldowns
/// - Hit detection (CircleCast) and target handling (Switch, Enemy, Projectile)
/// - Visual/effect activation window
/// - Also handles relevant animation and state changes
/// 
/// Note:
/// - Idk add a gun or something later.
/// - Any movement-related consequences (self-knockback/recoil) are delegated to PlayerMovement.
/// - Hit registration is handled here, but consequences (damage, status effects) are handled by the target objects.
/// </summary>
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class PlayerAttack : BaseEntity
{
    #region Fields
    [Header("Attack Stats")]
    [SerializeField] float maxComboDelay = 0.4f;
    [SerializeField] float slashIntervalTime = 0.2f;
    [SerializeField] int slashDamage = 1;

    [SerializeField] ContactFilter2D enemyContactFilter;
    [Header("Hitboxes")]
    [SerializeField] GameObject slash;
    [SerializeField] GameObject altSlash;
    [SerializeField] GameObject downSlash;
    [SerializeField] GameObject upSlash;
    [SerializeField] GameObject cycloneSlash;
    [SerializeField] GameObject wallSlash;
    [SerializeField] GameObject greatSlash;
    [SerializeField] GameObject dashSlash;
    [SerializeField] GameObject sharpShadow;

    AnimationController anim;
    PlayerInputHandler inputHandler;
    PlayerMovement movement;

    private int slashCount;
    private float lastSlashTime;
    public enum AttackType
    {
        Slash, AltSlash, DownSlash, UpSlash, CycloneSlash, WallSlash, GreatSlash, DashSlash, SharpShadow,
    }
    #endregion
    #region Methods
    protected override void Awake()
    {
        base.Awake();
        inputHandler = GetComponent<PlayerInputHandler>();
        anim = GetComponent<AnimationController>();
        movement = GetComponent<PlayerMovement>();

        if (inputHandler != null)
        {
            inputHandler.AttackStarted += OnAttackStarted;
        }
    }

    void Update()
    {
        ResetComboTimer();
    }

    private void OnAttackStarted(UnityEngine.InputSystem.InputAction.CallbackContext ctx)
    {
        if (!gameManager.IsEnableInput() || GetIsDead()) return;
        if (Time.time < lastSlashTime + slashIntervalTime) return;

        lastSlashTime = Time.time;

        var vectorInput = inputHandler.MoveInput;
        if (vectorInput.y > 0)
        {
            DoSlash(AttackType.UpSlash, "UpSlash");
        }
        else if (!movement.GetIsOnGround() && vectorInput.y < 0)
        {
            DoSlash(AttackType.DownSlash, "DownSlash");
        }
        else
        {
            slashCount++;
            if (slashCount == 1)
            {
                DoSlash(AttackType.Slash, "Slash");
            }
            else if (slashCount == 2)
            {
                DoSlash(AttackType.AltSlash, "AltSlash");
                slashCount = 0;
            }
        }
    }

    private void DoSlash(AttackType type, string animClip)
    {
        List<Collider2D> colliders = new List<Collider2D>();
        attacker.Play(type, ref colliders);

        bool hasEnemy = colliders.Any(c => c.gameObject.layer == LayerMask.NameToLayer("Enemy Detector"));
        bool hasDamageAll = colliders.Any(c => c.gameObject.layer == LayerMask.NameToLayer("Damage All"));

        if (hasEnemy)
        {
            if (type == AttackType.DownSlash) movement.AddDownRecoilForce();
            else StartCoroutine(movement.AddRecoilForce());
        }
        if (hasDamageAll)
        {
            if (type == AttackType.DownSlash)
            {
                audioEffectPlayer?.PlayOneShot(PlayerAudio.AudioClipType.SwordHitReject);
                movement.AddDownRecoilForce();
            }
        }
        /*
        foreach (Collider2D col in colliders)
        {
            var breakable = col.GetComponent<Breakable>();
            if (breakable != null) breakable.Hurt(slashDamage, transform);
        }*/

        if (anim != null) anim.Play(animClip);
    }

    private void ResetComboTimer()
    {
        if (Time.time >= lastSlashTime + maxComboDelay && slashCount != 0) slashCount = 0;
    }

    public IEnumerator TakeDamage(Enemy enemy = null)
    {
        gameManager.SetEnableInput(false);
        audioEffectPlayer?.Play(PlayerAudio.AudioType.HeroDamage, true);
        FindFirstObjectByType<HealthUI>()?.Hurt();
        if (!GetIsDead())
        {
            StartCoroutine(FindFirstObjectByType<Invincibility>().SetInvincibility());
            movement.ApplyDamageMovement();
        }
        if (anim != null) anim.Play("Damage");
        yield return null;
    }
    public void Play(AttackType attackType, ref List<Collider2D> colliders)
    {
        switch (attackType)
        {
            case AttackType.Slash:
                Physics2D.OverlapCollider(slash.GetComponent<Collider2D>(), enemyContactFilter, colliders);
                slash.GetComponent<AudioSource>().Play();
                break;
            case AttackType.AltSlash:
                Physics2D.OverlapCollider(altSlash.GetComponent<Collider2D>(), enemyContactFilter, colliders);
                altSlash.GetComponent<AudioSource>().Play();
                break;
            case AttackType.DownSlash:
                Physics2D.OverlapCollider(downSlash.GetComponent<Collider2D>(), enemyContactFilter, colliders);
                downSlash.GetComponent<AudioSource>().Play();
                break;
            case AttackType.UpSlash:
                Physics2D.OverlapCollider(upSlash.GetComponent<Collider2D>(), enemyContactFilter, colliders);
                upSlash.GetComponent<AudioSource>().Play();
                break;
            case AttackType.CycloneSlash:
                break;
            case AttackType.WallSlash:
                break;
            case AttackType.GreatSlash:
                break;
            case AttackType.DashSlash:
                break;
            case AttackType.SharpShadow:
                break;
            default:
                break;
        }
    }
    #endregion
}