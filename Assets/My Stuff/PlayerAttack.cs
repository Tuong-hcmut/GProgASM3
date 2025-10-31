using UnityEngine;
using System.Collections;

/// <summary>
/// Handles player attack logic:
/// - Buffered attack input & cooldowns
/// - Hit detection (CircleCast) and target handling (Switch, Enemy, Projectile)
/// - Visual/effect activation window
/// 
/// Note:
/// - Idk add a gun or something later.
/// - Any movement-related consequences (self-knockback/recoil) are delegated to PlayerMovement.
/// - Hit registration is handled here, but consequences (damage, status effects) are handled by the target objects.
/// </summary>
public class PlayerAttack : MonoBehaviour
{
    [Header("Attack Settings")]
    [Tooltip("Minimum delay between two consecutive attacks.")]
    [SerializeField] private float attackCooldown = 0.3f;

    [Tooltip("How long the attack input can be buffered before execution.")]
    [SerializeField] private float attackBufferTime = 0.1f;

    [Tooltip("Active hitbox duration (seconds).")]
    [SerializeField] private float activeAttackTime = 0.1f;

    [Tooltip("Optional hitbox GameObject to enable during attack.")]
    [SerializeField] private GameObject attackHitbox;

    [Header("Attack Effects & Detection")]
    [SerializeField] private GameObject attackUpEffect;
    [SerializeField] private GameObject attackForwardEffect;
    [SerializeField] private GameObject attackDownEffect;
    [SerializeField] private float attackRadius = 0.6f;
    [SerializeField] private float attackDistance = 1.5f;
    [SerializeField] private LayerMask attackLayerMask;
    [SerializeField] private float attackEffectLifeTime = 0.05f;

    // NOTE: recoil fields removed from here — recoil determination and application
    // are now handled inside PlayerMovement (ApplyAttackRecoil). This keeps all
    // movement/velocity modifications inside PlayerMovement per architecture.

    // === [Runtime State — conventional] ===
    private bool canAttack = true;
    private float cooldownTimer = 0f;
    private float bufferCounter = 0f;

    // store requested vertical direction for buffered attack (-1..1)
    private float queuedVertical = 0f;

    private PlayerMovement movement;

    private void Awake()
    {
        movement = GetComponent<PlayerMovement>();
    }

    void Update()
    {
        HandleAttackBuffer();
        HandleCooldown();
    }

    /// <summary>
    /// Attempt an attack using a vertical direction hint.
    /// >0.5 => up attack, &lt;-0.5 => down attack, otherwise forward.
    /// </summary>
    /// <param name="verticalInput">Vertical axis value from input layer.</param>
    public void TryAttack(float verticalInput)
    {
        if (canAttack)
        {
            PerformAttack(verticalInput);
        }
        else
        {
            // queue attack if within buffer window
            bufferCounter = attackBufferTime;
            queuedVertical = Mathf.Clamp(verticalInput, -1f, 1f);
        }
    }

    private void HandleAttackBuffer()
    {
        if (bufferCounter > 0f)
        {
            bufferCounter -= Time.deltaTime;

            // if buffer still active and attack becomes available, execute immediately
            if (canAttack)
            {
                PerformAttack(queuedVertical);
                bufferCounter = 0f;
            }
        }
    }

    private void HandleCooldown()
    {
        if (!canAttack)
        {
            cooldownTimer -= Time.deltaTime;
            if (cooldownTimer <= 0f)
                canAttack = true;
        }
    }

    private void PerformAttack(float verticalInput)
    {
        canAttack = false;
        cooldownTimer = attackCooldown;

        // Start coroutine for timing attack animation / hitbox window and detection
        StartCoroutine(AttackRoutine(verticalInput));
    }

    private IEnumerator AttackRoutine(float verticalInput)
    {
        // enable hitbox if provided (legacy support)
        if (attackHitbox != null)
            attackHitbox.SetActive(true);

        // choose effect & detection direction by verticalInput
        GameObject activeEffect = null;
        Vector2 detectDirection = Vector2.zero;

        if (verticalInput > 0.5f)
        {
            activeEffect = attackUpEffect;
            detectDirection = Vector2.up;
        }
        else if (verticalInput < -0.5f)
        {
            activeEffect = attackDownEffect;
            detectDirection = Vector2.down;
        }
        else
        {
            activeEffect = attackForwardEffect;
            float facing = transform.localScale.x;
            detectDirection = new Vector2(-facing, 0f);
        }

        if (activeEffect != null)
            activeEffect.SetActive(true);

        // perform detection immediately (instant attack window)
        Vector2 origin = (Vector2)transform.position;
        RaycastHit2D[] hits = Physics2D.CircleCastAll(origin, attackRadius, detectDirection, attackDistance, attackLayerMask);

        foreach (var hit in hits)
        {
            if (!hit.collider) continue;
            GameObject obj = hit.collider.gameObject;
            string layerName = LayerMask.LayerToName(obj.layer);

            if (layerName == "Switch")
            {
                // attempt to call turnOn if present
                var sw = obj.GetComponent<MonoBehaviour>();
                sw?.Invoke("turnOn", 0f);
            }
            else if (layerName == "Enemy")
            {
                // prefer BaseEntity if available
                var be = obj.GetComponent<BaseEntity>();
                if (be != null)
                {
                    be.TakeDamage(1);
                }
                else
                {
                    // fallback: try a common method name
                    var enemy = obj.GetComponent<MonoBehaviour>();
                    enemy?.Invoke("hurt", 0);
                }
            }
            else if (layerName == "Projectile")
            {
                Destroy(obj);
            }
        }

        // if we hit at least one target, request PlayerMovement to apply recoil
        if (hits.Length > 0 && movement != null)
        {
            movement.ApplyAttackRecoil(verticalInput);
        }

        yield return new WaitForSeconds(attackEffectLifeTime);

        if (activeEffect != null)
            activeEffect.SetActive(false);

        if (attackHitbox != null)
            attackHitbox.SetActive(false);

        // allow the active attack window to finish before returning (caller still enforces cooldown)
        yield return new WaitForSeconds(Mathf.Max(0f, activeAttackTime - attackEffectLifeTime));
    }
}
