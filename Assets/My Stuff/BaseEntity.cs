using UnityEngine;

/// <summary>
/// BaseEntity — shared health and combat foundation for all entities (Player, Enemies, Bosses).
/// Does not include movement, AI, or input — those belong to separate components.
/// </summary>
[RequireComponent(typeof(Collider2D))]
[DisallowMultipleComponent]
public abstract class BaseEntity : MonoBehaviour
{
    // === [Serialized Fields — user-defined] ===
    [Header("Stats")]
    [SerializeField] private int maxHealth = 3;          // total HP
    [SerializeField] private float invulnDuration = 0.5f; // invulnerability time after hit

    [Header("FX / Feedback")]
    [SerializeField] private GameObject hitEffect;        // hit visual
    [SerializeField] private AudioClip hitSound;          // hit SFX
    [SerializeField] private AudioClip deathSound;        // death SFX

    // === [Internal State — conventional] ===
    protected int currentHealth;                          // current HP
    protected bool isInvulnerable;                        // hit immunity flag
    protected float invulnTimer;                          // countdown timer

    // === [Cached Components — conventional] ===
    protected Collider2D hitbox;                          // damage collider
    protected AudioSource audioSource;                    // for SFX playback

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

    /// <summary>
    /// Increase HP by 1, clamped to maxHealth.
    /// </summary>
    public virtual void IncrementHealth()
    {
        currentHealth = Mathf.Clamp(currentHealth + 1, 0, maxHealth);
    }

    /// <summary>
    /// Decrease HP by 1 and trigger hit/death reactions.
    /// </summary>
    public virtual void DecrementHealth()
    {
        if (isInvulnerable || !IsAlive) return;

        currentHealth = Mathf.Clamp(currentHealth - 1, 0, maxHealth);

        if (currentHealth <= 0)
        {
            Die();
        }
        else
        {
            OnHit();
        }
    }

    /// <summary>
    /// Instantly deplete HP and handle death.
    /// </summary>
    public virtual void Die()
    {
        currentHealth = 0;
        OnDeath();
    }

    // === [Protected Internals] ===

    protected virtual void OnHit()
    {
        isInvulnerable = true;
        invulnTimer = invulnDuration;

        if (hitEffect)
            Instantiate(hitEffect, transform.position, Quaternion.identity);

        if (hitSound)
            audioSource?.PlayOneShot(hitSound);
    }

    protected virtual void OnDeath()
    {
        if (deathSound)
            audioSource?.PlayOneShot(deathSound);

        // Optionally disable collider or play death animation before destroying.
        hitbox.enabled = false;

        // Delayed destruction for SFX to finish.
        Destroy(gameObject, 0.2f);
    }

    protected void UpdateInvulnerability()
    {
        if (!isInvulnerable) return;

        invulnTimer -= Time.deltaTime;
        if (invulnTimer <= 0f)
        {
            isInvulnerable = false;
        }
    }

    // === [Utility Accessors] ===
    public int CurrentHealth => currentHealth;
    public int MaxHealth => maxHealth;
}
