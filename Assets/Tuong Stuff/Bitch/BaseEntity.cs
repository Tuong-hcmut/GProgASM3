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
using System.Collections;
using Unity.IO.LowLevel.Unsafe;
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
    protected bool isFacingLeft = false;
    readonly Quaternion flippedScale = Quaternion.Euler(0, 180, 0);
    readonly Quaternion normalScale = Quaternion.Euler(0, 0, 0);
    #endregion
    #region Methods

    protected virtual void Awake()
    {
        if (animator == null) animator = GetComponent<Animator>();
        if (rb == null) rb = GetComponent<Rigidbody2D>();
        if (gameManager == null) gameManager = FindFirstObjectByType<GameManager>();
    }
    //placeholder methods
    public void SetEnableInput(bool input) { gameManager.SetEnableInput(input); }
    public bool IsEnableInput() { return gameManager.IsEnableInput(); }
    public void TurnLeft()
    {
        isFacingLeft = true;
        transform.rotation = flippedScale;
    }
    public void TurnRight()
    {
        isFacingLeft = false;
        transform.rotation = normalScale;
    }
    protected void Flip()
    {
        if (isFacingLeft)
        {
            TurnRight();
        }
        else TurnLeft();
    }
    public virtual void Hurt(int damage)
    {
        if (isInvincible || isDead) return;

        health -= Mathf.Abs(damage);
        //FindFirstObjectByType<HealthUI>()?.OnEntityHurt(this);

        if (GetIsDead()) return;

        //StartCoroutine(FindFirstObjectByType<Invincibility>().SetInvincibility());
        //animator?.Play("Hurt");
    }
    public virtual void Hurt(int damage, Transform damagesource)
    {
        Hurt(damage);
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
        if (health <= 0 && !isDead)
        {
            Die();
        }
        return isDead;
    }

    public virtual void Die()
    {
        isDead = true;
        animator.Play("Dead");
    }
    protected IEnumerator DestroyAfterAnimation()
    {
        print("DestroyAfterAnimation called");
        // Wait for the current "Dead" animation to finish
        float deathAnimLength = 0.5f;
        if (animator.GetCurrentAnimatorStateInfo(0).IsName("Dead"))
            deathAnimLength = animator.GetCurrentAnimatorStateInfo(0).length;
        print(deathAnimLength);
        yield return new WaitForSeconds(deathAnimLength);
        print("Destroy called");
        Destroy(gameObject);
    }

    public void Respawn()
    {
        isDead = false;
        health = (health > 0) ? health : maxHealth;
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
    public bool GetIsFacingLeft() => isFacingLeft;
}