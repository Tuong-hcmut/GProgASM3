using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

[RequireComponent(typeof(Animator), typeof(AudioSource), typeof(Rigidbody2D))]
public class Enemy : BaseEntity
{
    protected PlayerAttack character;
    protected AudioSource audioSource;
    protected Transform player;

    [Header("Movement")]
    [SerializeField] protected float movementSpeed = 2f;
    protected bool canMove = true;
    protected bool isChasing = false;
    [Header("Roam Settings")]
    [SerializeField] private float roamSpeed = 1f;
    [SerializeField] private float roamChangeInterval = 2f;

    private float roamTimer;
    protected Vector2 roamDirection;

    [Header("Audio Clip")]
    [SerializeField] protected AudioClip enemyDeathSword;

    [Header("Forces")]
    [SerializeField] protected float hurtForce = 300f;
    [SerializeField] protected float deadForce = 500f;

    [Header("Coin Attr")]
    [SerializeField] protected GameObject coin;
    [SerializeField] protected int minSpawnCount = 1;
    [SerializeField] protected int maxSpawnCount = 4;
    [SerializeField] protected float maxBumpHorizontalForce = 400;
    [SerializeField] protected float minBumpVerticalForce = 600;
    [SerializeField] protected float maxBumpVerticalForce = 800;

    [Header("Vision Cone")]
    [SerializeField] protected GameObject visionCone; // assign in Inspector

    protected override void Awake()
    {
        base.Awake();
        audioSource = GetComponent<AudioSource>();
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        character = FindFirstObjectByType<PlayerAttack>();

        if (visionCone != null)
        {
            var vc = visionCone.GetComponent<VisionCone>();
            vc.owner = this;
        }
    }

    protected virtual void Update()
    {
        if (GetIsDead()) return;

        if (isChasing && player != null)
        { Chase(); }
        else Roam();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (GetIsDead()) return;
        if (collision.gameObject.layer == LayerMask.NameToLayer("Hero Detector"))
            character?.Hurt(1);
    }

    public override void Hurt(int damage, Transform attackPosition)
    {
        base.Hurt(damage);

        if (GetIsDead()) return;
        if (player == null || rb == null) return;

        Vector2 diff = transform.position - player.position;
        animator?.SetTrigger("Hurt");
        rb.linearVelocity = Vector2.zero;

        rb.AddForce(new Vector2(diff.x > 0 ? hurtForce : -hurtForce, 0));
    }

    public override void Die()
    {
        print("enemy die called");
        base.Die();
        if (player == null || rb == null) return;

        audioSource?.PlayOneShot(enemyDeathSword);
        Vector3 diff = (player.position - transform.position).normalized;
        rb.linearVelocity = Vector2.zero;
        rb.gravityScale = 3;

        rb.AddForce(diff.x > 0 ? Vector2.left * deadForce : Vector2.right * deadForce);

        print("enemy knocked back");
        SpawnCoins();
        print("Coin Spawned");
        StartCoroutine(DestroyAfterAnimation());
    }

    public virtual void SpawnCoins()
    {
        if (coin == null) return;

        int randomCount = Random.Range(minSpawnCount, maxSpawnCount + 1);
        for (int i = 0; i < randomCount; i++)
        {
            GameObject geo = Instantiate(coin, transform.position, Quaternion.identity, transform.parent);
            Vector2 force = new Vector2(
                Random.Range(-maxBumpHorizontalForce, maxBumpHorizontalForce),
                Random.Range(minBumpVerticalForce, maxBumpVerticalForce)
            );
            geo.GetComponent<Rigidbody2D>()?.AddForce(force, ForceMode2D.Force);
        }
    }

    // ---- Vision Cone Callbacks ----
    public virtual void OnVisionEnter(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isChasing = true;
            OnChaseStart();
        }
    }

    public virtual void OnVisionExit(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isChasing = false;
            OnChaseStop();
        }
    }

    // ---- Overridable Chase Logic ----
    protected virtual void Chase()
    {
        if (player == null || rb == null) return;

        Vector2 dir = (player.position - transform.position).normalized;
        rb.linearVelocity = new Vector2(dir.x * movementSpeed, rb.linearVelocity.y);
        if (dir.x > 0)
            TurnRight();
        else TurnLeft();
    }
    protected virtual void Roam()
    {
        if (rb == null) return;

        roamTimer -= Time.deltaTime;
        if (roamTimer <= 0)
        {
            roamTimer = roamChangeInterval;
            roamDirection = Random.insideUnitCircle.normalized;
            roamDirection.y = 0; // constrain to horizontal
        }

        rb.linearVelocity = new Vector2(roamDirection.x * roamSpeed, rb.linearVelocity.y);
        if (roamDirection.x > 0) TurnRight();
        else if (roamDirection.x < 0) TurnLeft();
    }

    protected virtual void OnChaseStart() { Chase(); }
    protected virtual void OnChaseStop()
    {
        rb.linearVelocity = Vector2.zero;
        Roam();
    }
}