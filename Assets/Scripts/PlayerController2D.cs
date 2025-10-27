
using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController2D : MonoBehaviour
{
    // --- THÊM BIẾN KIỂM SOÁT INPUT ---
    [Header("Input Setup")]
    public bool useArrowKeys = false; // true = Mũi tên, false = WASD (Default)
    // ---------------------------------
    [Header("AI = 0, Player 1 & 2 = 1 & 2")]
    public int controlledByPlayer = 0; // 0 = AI, 1 = Player1, 2 = Player2

    [Header("Movement Settings (Arcade Top-Down)")]
    public float maxSpeed = 50f;            // Tốc độ tối đa
    public float acceleration = 30f;       // Lực đẩy khi nhấn nút

    [Header("Drift & Steering")]
    public float steerSpeed = 360f;        // Tốc độ xoay thân xe
    [Range(0f, 1f)]
    public float driftFactor = 0.95f;      // Hệ số giữ vận tốc ngang
    [Range(0f, 1f)]
    public float turnDriftFactor = 0.8f;   // Hệ số giữ vận tốc ngang khi lái gấp

    [Header("Boost Settings")]
    public float boostMultiplier = 3.5f;      // Tăng tốc gấp 2 lần
    public float boostDuration = 0.3f;        // Thời gian tăng tốc (giây)
    public bool canBoost = false;          // Xe có thể tăng tốc hay không
    private bool isBoosting = false;        // Đang boost
    private float boostTimer = 0f;
    private Rigidbody2D rb;
    public Collider2D playArea;
    protected Vector2 moveInput;
    protected float targetAngle;
    protected bool isMoving;
    private ItemSpawner spawner;
    private HUDManager hud;
    private const float DefaultLinearDamping = 0.1f;

    [Header("Audio")]
    [SerializeField] private AudioClip pickupSound;              // user-defined
    [SerializeField] private AudioClip boostSound;               // user-defined
    [SerializeField] private float pickupVolume = 0.6f;          // user-defined
    [SerializeField] private float boostVolume = 0.8f;           // user-defined

    private AudioSource audioSource;


    protected virtual void OnEnable()
    {
        if (spawner == null)
            spawner = FindAnyObjectByType<ItemSpawner>();
        if (rb == null)
        {
            rb = GetComponent<Rigidbody2D>();

            // Đặt lại các thuộc tính vật lý cần thiết
            if (rb != null)
            {
                rb.gravityScale = 0f;
                rb.constraints = RigidbodyConstraints2D.None;
                rb.linearDamping = DefaultLinearDamping;
            }
        }
        // Đặt lại input để tránh xe tự di chuyển nếu người chơi đang giữ phím
        moveInput = Vector2.zero;
        isMoving = false;
    }

    void Reset()
    {
        maxSpeed = 50f;
        acceleration = 30f;
        steerSpeed = 360f;
        driftFactor = 0.95f;
        turnDriftFactor = 0.8f;

        // Setup Rigidbody cho Reset
        rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.gravityScale = 0f;
            rb.constraints = RigidbodyConstraints2D.None;
            rb.linearDamping = 0.1f; // Giá trị damping thấp cho quán tính
        }
    }

    protected virtual void Awake()
    {

        if (rb == null)
        {
            rb = GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.gravityScale = 0f;
                rb.constraints = RigidbodyConstraints2D.None;
                rb.linearDamping = DefaultLinearDamping;
            }
        }
        audioSource = GetComponent<AudioSource>();
        hud = FindFirstObjectByType<HUDManager>();
        if (!gameObject.CompareTag("Player"))
        {
            enabled = false;
        }
    }

    void Update()
    {
        // --- 1. Xử lý Input WASD/Mũi tên dựa trên biến useArrowKeys ---
        moveInput = Vector2.zero;
        var keyboard = Keyboard.current;

        if (keyboard != null)
        {
            if (!useArrowKeys)
            {
                // Input cho Player 1 (WASD)
                if (keyboard.wKey.isPressed) moveInput.y = 1f;
                if (keyboard.sKey.isPressed) moveInput.y = -1f;
                if (keyboard.aKey.isPressed) moveInput.x = -1f;
                if (keyboard.dKey.isPressed) moveInput.x = 1f;
            }
            else
            {
                // Input cho Player 2 (Mũi tên)
                if (keyboard.upArrowKey.isPressed) moveInput.y = 1f;
                if (keyboard.downArrowKey.isPressed) moveInput.y = -1f;
                if (keyboard.leftArrowKey.isPressed) moveInput.x = -1f;
                if (keyboard.rightArrowKey.isPressed) moveInput.x = 1f;
            }
        }

        moveInput.Normalize();
        isMoving = moveInput != Vector2.zero;

        // --- 2. Tính Toán Góc Quay Mục Tiêu (giữ nguyên) ---
        if (isMoving)
        {
            targetAngle = Mathf.Atan2(moveInput.y, moveInput.x) * Mathf.Rad2Deg - 90f;
        }
        if (keyboard != null)
        {
            bool boostPressed = false;

            // Player 1 uses Left Shift, Player 2 uses Right Shift
            if (controlledByPlayer == 1 && keyboard.leftShiftKey.wasPressedThisFrame)
                boostPressed = true;
            else if (controlledByPlayer == 2 && keyboard.rightShiftKey.wasPressedThisFrame)
                boostPressed = true;

            if (boostPressed)
            {
                if (canBoost)
                {
                    isBoosting = true;
                    boostTimer = boostDuration;
                    canBoost = false;
                    if (boostSound != null)
                        audioSource.PlayOneShot(boostSound, boostVolume);
                    if (hud != null)
                        hud.UpdateHUD();
                    Debug.Log($"[{name}] Boost STARTED at {Time.time:F2}s (Duration: {boostDuration}s)");
                }
                else { Debug.LogWarning($"[{name}] Tried to boost but no boost available at {Time.time:F2}s"); }
            }
        }
    }

    // Các hàm FixedUpdate, ApplySteering, ApplyMovement, ApplyDrift giữ nguyên.
    void FixedUpdate()
    {
        if (rb == null || controlledByPlayer == 0) return;

        ApplySteering();
        ApplyMovement();
        ApplyDrift();

        if (isBoosting)
        {
            // Rocket League–style dash: strong impulse in forward direction
            Vector2 dashDir = transform.up.normalized;
            float dashForce = acceleration * boostMultiplier;

            rb.AddForce(dashDir * dashForce, ForceMode2D.Impulse);

            // Optional: slightly reduce drag while boosting
            rb.linearDamping = 0.02f;

            boostTimer -= Time.fixedDeltaTime;
            if (boostTimer <= 0f)
            {
                isBoosting = false;
                rb.linearDamping = DefaultLinearDamping; // reset drag
                if (playArea != null && !playArea.OverlapPoint(transform.position))
                {
                    Vector2 corrected = GetNearestPointInsidePlayArea(transform.position);
                    transform.position = corrected;
                    rb.linearVelocity = Vector2.zero; // optional: reset speed
                    rb.angularVelocity = 0f;
                }
            }
        }

        // Normal movement only when not boosting
        else if (isMoving)
        {
            Vector2 force = moveInput * acceleration;
            rb.AddForce(force, ForceMode2D.Force);

            // Enforce speed cap only in normal mode
            if (rb.linearVelocity.magnitude > maxSpeed)
            {
                rb.linearVelocity = rb.linearVelocity.normalized * maxSpeed;
            }
        }
    }

    // --- A. Xoay Thân Xe (Luôn xoay về hướng Input) ---
    protected void ApplySteering()
    {
        if (isMoving)
        {
            float currentAngle = transform.eulerAngles.z;
            // Xoay mượt về targetAngle
            float newAngle = Mathf.MoveTowardsAngle(currentAngle, targetAngle, steerSpeed * Time.fixedDeltaTime);
            rb.rotation = newAngle; // Dùng rb.rotation thay vì transform.rotation
        }
    }

    // --- B. Áp Dụng Lực (Dựa trên Input) ---
    protected void ApplyMovement()
    {
        if (isMoving)
        {
            // Áp dụng lực theo hướng INPUT (moveInput)
            Vector2 force = moveInput * acceleration;
            rb.AddForce(force, ForceMode2D.Force);

            // Giới hạn tốc độ
            if (rb.linearVelocity.magnitude > maxSpeed)
            {
                rb.linearVelocity = rb.linearVelocity.normalized * maxSpeed;
            }
        }
        else
        {
            // Tăng damping khi không nhấn nút để xe dừng lại mượt hơn
            // (rb.linearDamping đã set sẵn trong Awake, có thể bỏ qua bước này nếu linearDamping đã đủ)
        }
    }

    // --- C. Áp Dụng Drift & Tạo Vòng Cung ---
    protected void ApplyDrift()
    {
        Vector2 forwardVelocity = transform.up * Vector2.Dot(rb.linearVelocity, transform.up);
        Vector2 rightVelocity = transform.right * Vector2.Dot(rb.linearVelocity, transform.right);

        float angleDiff = Mathf.Abs(Mathf.DeltaAngle(rb.rotation, targetAngle));

        // Logic này đảm bảo khi góc lệch lớn (đổi hướng gấp), turnDriftFactor thấp sẽ được áp dụng
        float t = Mathf.InverseLerp(45f, 180f, angleDiff);
        float appliedDrift = Mathf.Lerp(driftFactor, turnDriftFactor, t);

        // Giảm vận tốc ngang (rightVelocity) -> Tăng trượt đuôi
        rightVelocity *= appliedDrift;

        rb.linearVelocity = forwardVelocity + rightVelocity;
    }

    protected virtual void OnTriggerEnter2D(Collider2D collision)
    {
        if (canBoost) return;
        if (collision.CompareTag("Collectible"))
        {
            canBoost = true;
            if (pickupSound != null)
                audioSource.PlayOneShot(pickupSound, pickupVolume);
            if (hud != null)
                hud.UpdateHUD();
            Debug.Log($"[{name}] Collected boost item at {Time.time:F2}s");

            // Inform the spawner that an item was removed
            if (spawner != null)
            {
                spawner.NotifyItemRemoved(collision.gameObject);
            }

            // Destroy the collected item so the spawner replaces it
            Destroy(collision.gameObject);
        }
    }
    private Vector2 GetNearestPointInsidePlayArea(Vector2 outsidePos)
    {
        if (playArea == null) return outsidePos;

        Vector2 center = playArea.bounds.center;
        Vector2 direction = (outsidePos - center).normalized;

        RaycastHit2D hit = Physics2D.Raycast(center, direction, Mathf.Infinity, LayerMask.GetMask("Default"));
        if (hit.collider == playArea)
        {
            // Move slightly inward from the border
            Vector2 inwardOffset = -direction * 0.5f;
            return hit.point + inwardOffset;
        }

        // Fallback: just clamp inside the bounding box if no intersection
        Bounds b = playArea.bounds;
        Vector2 clamped = new Vector2(
            Mathf.Clamp(outsidePos.x, b.min.x + 0.5f, b.max.x - 0.5f),
            Mathf.Clamp(outsidePos.y, b.min.y + 0.5f, b.max.y - 0.5f)
        );
        return clamped;
    }
}