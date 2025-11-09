using System.Collections;
using UnityEngine;
using UnityEngine.UI; // Cần thiết cho UI Slider

public class BigBoss : MonoBehaviour
{
    [Header("Health & Stats")]
    public int maxHealth = 3; // Cần 3 cú dậm để thắng
    private int currentHealth;
    public Slider healthBar; // Kéo Slider "BossHealthBar" vào đây
    public GameObject stunIndicator; // (Tùy chọn) Một icon "!" để hiện khi bị choáng

    [Header("AI & Vision")]
    public Transform playerTransform;
    private bool isPlayerInSight = false;
    private bool isVulnerable = false; // Trạng thái choáng

    [Header("Fireball Attack")]
    public GameObject fireballPrefab; // Prefab của quả cầu lửa
    public Transform fireballSpawnPoint; // Vị trí (ví dụ: tay) boss ném lửa
    public float fireballCooldown = 3f; // 3 giây ném 1 lần
    private bool canThrowFireball = true;

    [Header("Jump Slam Attack")]
    public float jumpSlamInterval = 10f; // 10 giây làm 1 lần
    public float jumpForce = 15f;
    public float slamRadius = 3f; // Bán kính sát thương khi đáp
    public int slamDamage = 30; // Sát thương gây cho player
    public float vulnerabilityDuration = 3f; // Bị choáng 3 giây
    private Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        currentHealth = maxHealth;
        UpdateHealthBar();

        if (playerTransform == null)
        {
            playerTransform = GameObject.FindGameObjectWithTag("Player").transform;
        }

        // Bắt đầu 2 hành vi tấn công
        StartCoroutine(JumpSlamRoutine());
        StartCoroutine(FireballRoutine());

        if (stunIndicator != null) stunIndicator.SetActive(false);
    }

    // ----- CÁC HÀNH VI TẤN CÔNG -----

    // Coroutine cho việc ném cầu lửa
    private IEnumerator FireballRoutine()
    {
        while (true)
        {
            // Chờ cho đến khi player vào tầm nhìn VÀ boss sẵn sàng ném
            yield return new WaitUntil(() => isPlayerInSight && canThrowFireball);

            // Ném
            if (!isVulnerable) // Đừng ném khi đang bị choáng
            {
                ThrowFireball();
                canThrowFireball = false; // Bắt đầu cooldown
                yield return new WaitForSeconds(fireballCooldown);
                canThrowFireball = true; // Hết cooldown
            }
        }
    }

    private void ThrowFireball()
    {
        if (fireballPrefab == null || playerTransform == null) return;
        
        Debug.Log("Boss: Throwing Fireball!");
        GameObject fireball = Instantiate(fireballPrefab, fireballSpawnPoint.position, Quaternion.identity);
        
        // Tính toán hướng bay
        Vector2 direction = (playerTransform.position - fireballSpawnPoint.position).normalized;
        fireball.GetComponent<Fireball>().Setup(direction); // Gửi hướng đi cho script Fireball
    }

    // Coroutine cho việc nhảy và dậm
    private IEnumerator JumpSlamRoutine()
    {
        while (true)
        {
            // Chờ hết thời gian
            yield return new WaitForSeconds(jumpSlamInterval);
            
            if(isVulnerable) yield return null; // Bỏ qua nếu đang bị choáng

            Debug.Log("Boss: Preparing Jump Slam!");
            // 1. Nhảy lên
            rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);

            // 2. Chờ cho đến khi đáp xuống đất (Bạn cần có GroundCheck cho boss)
            // (Giả sử bạn có hàm IsGrounded() giống như Crawler)
            yield return new WaitForSeconds(1.0f); // Chờ bay lên
            yield return new WaitUntil(() => rb.linearVelocity.y < 0.1f && IsGrounded()); // Đợi đến khi chạm đất
            
            // 3. Gây sát thương AOE khi đáp
            SlamAttack();

            // 4. Trở nên bị choáng (Vulnerable)
            SetVulnerable(true);
            yield return new WaitForSeconds(vulnerabilityDuration);
            SetVulnerable(false);
        }
    }
    
    // (Bạn cần tự implement hàm này dựa trên groundCheck của boss)
    private bool IsGrounded()
    {
        // Tương tự như 'isGrounded' của Crawler
        // Ví dụ: return Physics2D.OverlapCircle(groundCheck.transform.position, circleRadius, groundLayer);
        return true; // Tạm thời
    }

    private void SlamAttack()
    {
        Debug.Log("Boss: SLAM!");
        // Tạo một vòng tròn vô hình tại chân boss
        Collider2D[] hitObjects = Physics2D.OverlapCircleAll(transform.position, slamRadius);
        foreach (Collider2D obj in hitObjects)
        {
            // Nếu player ở trong vòng tròn
            if (obj.CompareTag("Player"))
            {
                Debug.Log("Slam hit Player!");
                obj.GetComponent<PlayerStats>().TakeDamage(slamDamage);
            }
        }
    }

    // ----- CÁC HÀNH VI KHÁC -----

    // Được gọi bởi VisionCone.cs
    public void OnVisionEnter(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInSight = true;
        }
    }

    public void OnVisionExit(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInSight = false;
        }
    }
    
    private void SetVulnerable(bool state)
    {
        isVulnerable = state;
        Debug.Log("Boss vulnerable: " + state);
        if (stunIndicator != null) stunIndicator.SetActive(state);
    }
    
    // Hàm này cho PlayerCollision gọi
    public bool IsVulnerable()
    {
        return isVulnerable;
    }

    public void TakeDamage(int damage)
    {
        // Chỉ nhận sát thương khi đang bị choáng
        if (!isVulnerable) return;

        currentHealth -= damage;
        UpdateHealthBar();
        Debug.Log("Boss took damage! Health: " + currentHealth);

        // Bị đánh 1 phát là hết choáng ngay
        SetVulnerable(false);

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        Debug.Log("Boss defeated!");
        // Dừng mọi hành động
        GameManager.Instance.OnPlayerWin();
        StopAllCoroutines();
        this.enabled = false;
        // Tắt UI
        if(healthBar != null) healthBar.gameObject.SetActive(false);
        // (Thêm animation chết, âm thanh, v.v.)
        
        Destroy(gameObject, 2f); // Biến mất sau 2 giây
    }

    private void UpdateHealthBar()
    {
        if (healthBar != null)
        {
            healthBar.value = (float)currentHealth / maxHealth;
        }
    }
}