using UnityEngine;

/// <summary>
/// BaseEntity — shared health, damage, and death handling for all living entities (Player, Enemies, Bosses).
/// Responsibilities:
///  - Track HP, invulnerability windows, and play hit/death FX/SFX.
///  - Expose TakeDamage/Heal/Die so external systems (AI, attacks) can interact.
/// </summary>
[RequireComponent(typeof(Collider2D))]
[DisallowMultipleComponent]
public abstract class BaseEntity : MonoBehaviour
{
    // === [Serialized Fields — user-defined] ===
    [Header("Stats")]
    [SerializeField] private int maxHealth = 3;
    [SerializeField] private float invulnDuration = 0.5f; // invulnerability time after hit
    [SerializeField] private float deathDelay = 0.25f;    // time before destruction (for SFX/animation purposes)
    [Header("FX / Feedback")]
    [SerializeField] private GameObject hitEffect;
    [SerializeField] private AudioClip hitSound;
    [SerializeField] private AudioClip deathSound;
    [SerializeField] private Color hurtColor = Color.red;
    [SerializeField] private Color normalColor = Color.white;   // default color

    // === [Internal State — conventional] ===
    protected int currentHealth;
    protected bool isInvulnerable;
    protected float invulnTimer;

    // === [Cached Components — conventional] ===
    protected Collider2D hitbox;                          // damage collider
    protected AudioSource audioSource;
    protected SpriteRenderer sprite;

    // === [Unity Lifecycle] ===
    protected virtual void Awake()
    {
        hitbox = GetComponent<Collider2D>();
        audioSource = GetComponent<AudioSource>();
        currentHealth = maxHealth;
    }

    protected virtual void Update()
    {
        UpdateInvulnerability();
    }

    // === [Public API] ===

    /// <summary>
    /// Returns true if the entity has more than 0 HP.
    /// </summary>
    public bool IsAlive => currentHealth > 0;

    /// <summary> Returns current health value. </summary>
    public int CurrentHealth => currentHealth;

    /// <summary> Returns maximum health value. </summary>
    public int MaxHealth => maxHealth;

    /// <summary>
    /// Restore HP by a certain amount, clamped to maxHealth.
    /// </summary>
    public virtual void Heal(int amount)
    {
        if (!IsAlive) return;
        currentHealth = Mathf.Clamp(currentHealth + amount, 0, maxHealth);
    }

    /// <summary>
    /// Decrease HP by a certain amount and trigger hit/death reactions.
    /// </summary>
    public virtual void TakeDamage(int amount)
    {
        if (isInvulnerable || !IsAlive) return;

        currentHealth = Mathf.Clamp(currentHealth - amount, 0, maxHealth);

        if (currentHealth <= 0)
            Die();
        else
            OnHit();
    }

    /// <summary>
    /// Instantly deplete HP and handle death.
    /// </summary>
    public virtual void Die()
    {
        if (!IsAlive) return;
        currentHealth = 0;
        OnDeath();
    }

    // === [Protected Internals] ===

    /// <summary>
    /// Default OnHit reaction: spawn hit FX, set invulnerability and flash sprite color.
    /// Override to add entity-specific behavior but avoid direct kinematic changes here.
    /// </summary>
    protected virtual void OnHit()
    {
        isInvulnerable = true;
        invulnTimer = invulnDuration;

        if (hitEffect)
            Instantiate(hitEffect, transform.position, Quaternion.identity);
        if (sprite)
            sprite.color = hurtColor;
        if (hitSound)
            audioSource?.PlayOneShot(hitSound);
    }

    /// <summary>
    /// Default OnDeath reaction: play sound, disable collider, then destroy after a delay.
    /// </summary>
    protected virtual void OnDeath()
    {
        if (deathSound)
            audioSource?.PlayOneShot(deathSound);

        // Optionally disable collider or play death animation before destroying.
        hitbox.enabled = false;

        // Let play death animations
        StartCoroutine(DelayedDestroy(deathDelay));
    }

    protected void UpdateInvulnerability()
    {
        if (!isInvulnerable) return;

        invulnTimer -= Time.deltaTime;
        if (invulnTimer <= 0f)
        {
            isInvulnerable = false;
            if (sprite)
                sprite.color = normalColor;
        }
    }
    protected virtual System.Collections.IEnumerator DelayedDestroy(float delay)
    {
        yield return new WaitForSeconds(delay);
        Destroy(gameObject);
    }
}
