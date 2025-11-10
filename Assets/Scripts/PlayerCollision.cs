using UnityEngine;

public class PlayerCollision : MonoBehaviour
{
    private PlayerStats playerStats;
    public float stompBounceForce = 10f;
    private Rigidbody2D rb;

    void Start()
    {
        playerStats = GetComponent<PlayerStats>();
        rb = GetComponent<Rigidbody2D>();
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (playerStats == null) return;

        // ƯU TIÊN 1: Dậm đầu Enemy thường
        if (collision.CompareTag("EnemyHead"))
        {
            Debug.Log("Stomped on enemy head!");
            Crawler enemy = collision.GetComponentInParent<Crawler>();
            if (enemy != null)
            {
                enemy.Die();
            }
            if (rb != null)
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0);
                rb.AddForce(Vector2.up * stompBounceForce, ForceMode2D.Impulse);
            }
        }

        // --- LOGIC MỚI CHO BOSS ---
        // ƯU TIÊN 2: Dậm đầu Boss
        else if (collision.CompareTag("BossHead"))
        {
            BigBoss boss = collision.GetComponentInParent<BigBoss>();
            if (boss != null && boss.IsVulnerable()) // Chỉ gây sát thương KHI BOSS BỊ CHOÁNG
            {
                Debug.Log("Stomped on BOSS head!");
                boss.TakeDamage(1); // Gây 1 "điểm" sát thương

                // Nảy lên
                if (rb != null)
                {
                    rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0);
                    rb.AddForce(Vector2.up * stompBounceForce, ForceMode2D.Impulse);
                }
            }
        }
        // --- KẾT THÚC LOGIC BOSS ---

        // ƯU TIÊN 3: Va chạm khác
        else if (collision.CompareTag("Enemy"))
        {
        }
        else if (collision.CompareTag("Trap"))
        {
            playerStats.TakeDamage(10);
            Debug.Log("Player hit a trap! -10 HP");
        }
        else if (collision.CompareTag("Water"))
        {
            playerStats.TakeDamage(15);
            Debug.Log("Player fell into water! -15 HP");
        }
    }
}