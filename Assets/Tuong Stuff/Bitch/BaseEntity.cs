/// <summary>
/// BaseEntity — shared health, damage, and death handling for all living entities (Player, Enemies, Bosses).
/// Responsibilities:
///  - Track HP, invulnerability windows, and play hit/death FX/SFX.
///  - Expose TakeDamage/Heal/Die so external systems (AI, attacks) can interact.
/// 
/// Note:
///  - Tell the other guy to make respawn points.
/// </summary>
using UnityEngine;

public class BaseEntity : MonoBehaviour
{
    #region Fields
    [Header("Fields")]
    [SerializeField] private int maxHealth = 5;
    [SerializeField] private int health;
    [SerializeField] private bool isInvincible;
    [SerializeField] private bool isDead;
    [SerializeField] protected Animator animator;
    [SerializeField] internal PlayerAudio audioEffectPlayer;
    //   [SerializeField] protected PlayerEffect effecter;
    [SerializeField] protected AudioSource audioMusicPlayer;
    [SerializeField] internal GameManager gameManager;

    protected Rigidbody2D rb;
    #endregion
    #region Methods

    protected virtual void Awake()
    {
        if (animator == null) animator = GetComponent<Animator>();
        if (rb == null) rb = GetComponent<Rigidbody2D>();
        if (gameManager == null) gameManager = FindFirstObjectByType<GameManager>();
    }
    private void CheckIsDead()
    {
        if (health <= 0 && !isDead)
        {
            Die();
        }
    }
    //placeholder methods
    public void SetEnableInput(bool input) { gameManager.SetEnableInput(input); }
    public bool IsEnableInput() { return gameManager.IsEnableInput(); }

    public virtual void Hurt(int damage)
    {
        if (isInvincible || isDead) return;

        health -= Mathf.Abs(damage);
        //FindFirstObjectByType<HealthUI>()?.OnEntityHurt(this);

        if (GetIsDead()) return;

        //StartCoroutine(FindFirstObjectByType<Invincibility>().SetInvincibility());
        //animator?.Play("Hurt");
    }

    public void SetInvincible(bool value)
    {
        isInvincible = value;
    }

    public int GetCurrentHealth()
    {
        return health;
    }

    public bool GetIsDead()
    {
        CheckIsDead();
        return isDead;
    }

    public void Die()
    {
        Physics2D.IgnoreLayerCollision(LayerMask.NameToLayer("Hero Detector"), LayerMask.NameToLayer("Enemy Detector"), true);
        isDead = true;
        animator.SetTrigger("Dead");
    }

    public void Respawn()
    {
        //     FindFirstObjectByType<HazardRespawn>().Respawn();
    }

    public void SetRespawnData(int health)
    {
        if (health > 0)
        {
            this.health = health;
            animator.ResetTrigger("Dead");
            isDead = false;
        }
    }
    #endregion

    public Animator GetAnimator() => animator;
    public PlayerAudio GetAudio() => audioEffectPlayer;
    //  public PlayerEffect GetEffecter() => effecter;
    public AudioSource GetMusicPlayer() => audioMusicPlayer;
    public GameManager GetGameManager() => gameManager;
    public Rigidbody2D GetRigidbody() => rb;
}