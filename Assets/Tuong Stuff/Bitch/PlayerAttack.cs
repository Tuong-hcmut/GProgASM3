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

    AnimationController anim;
    PlayerInputHandler inputHandler;
    PlayerMovement movement;

    private int slashCount;
    private float lastSlashTime;
    public enum AttackType
    {
        Slash, AltSlash, DownSlash, UpSlash
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
            DoSlash(AttackType.UpSlash, "UpAttack");
        }
        else if (!movement.GetIsOnGround() && vectorInput.y < 0)
        {
            DoSlash(AttackType.DownSlash, "DownAttack");
        }
        else
        {
            slashCount++;
            if (slashCount == 1)
            {
                DoSlash(AttackType.Slash, "Attack");
            }
            else if (slashCount == 2)
            {
                DoSlash(AttackType.AltSlash, "AltAttack");
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
        foreach (Collider2D col in colliders)
        {
            // enemy damage
            if (col.gameObject.layer == LayerMask.NameToLayer("Enemy Detector"))
            {
                // find the root enemy object, in case the hitbox is a child
                var enemy = col.GetComponentInParent<BaseEntity>();
                if (enemy != null && enemy != this && !enemy.GetIsDead())
                {
                    enemy.Hurt(slashDamage);
                }
                continue;
            }
        }
        if (anim != null) anim.Play(animClip);
    }
    private void ResetComboTimer()
    {
        if (Time.time >= lastSlashTime + maxComboDelay && slashCount != 0) slashCount = 0;
    }
    public override void Hurt(int damage)
    {
        base.Hurt(damage);
        TakeDamage();
    }
    public IEnumerator TakeDamage()
    {
        audioEffectPlayer?.Play(PlayerAudio.AudioType.HeroDamage, true);
        FindFirstObjectByType<HealthUI>()?.Hurt();
        if (!GetIsDead())
        {
            StartCoroutine(FindFirstObjectByType<Invincibility>().SetInvincibility());
            movement.ApplyDamageMovement();
        }
        if (anim != null) anim.Play("Hurt");
        gameManager.SetEnableInput(false);
        yield return new WaitForSeconds(0.5f);
        gameManager.SetEnableInput(true);
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
            default:
                break;
        }
    }
    #endregion
}