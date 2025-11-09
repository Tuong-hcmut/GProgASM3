using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Crawler : Enemy
{
    [SerializeField] AudioClip hited;
    [SerializeField] Collider2D facingDetector;
    [SerializeField] ContactFilter2D contactFilter;
    [SerializeField] LayerMask groundLayer;
    [SerializeField] GameObject groundCheck;
    [SerializeField] float circleRadius;
    bool isGrounded;

    protected override void Update()
    {
        base.Update();
        FacingDetect();
    }

    private void FacingDetect()
    {
        if (GetIsDead()) return;
        isGrounded = Physics2D.OverlapCircle(groundCheck.transform.position, circleRadius, groundLayer);
        if (!isGrounded)
        {
            Flip();
            roamDirection.x = -roamDirection.x;
        }
        else
        {
            int count = Physics2D.OverlapCollider(facingDetector, contactFilter, new List<Collider2D>());
            if (count > 0)
            {
                Flip();
                roamDirection.x = -roamDirection.x;
            }
        }
    }
    public void ItDie()
    {
        // Kiểm tra xem nó đã chết chưa, để tránh gọi hàm này nhiều lần
        if (GetIsDead()) return; 

        Debug.Log("Enemy was stomped and died!");

        // Tắt tất cả các collider để player không tương tác nữa
        Collider2D[] colliders = GetComponentsInChildren<Collider2D>();
        foreach (Collider2D col in colliders)
        {
            col.enabled = false;
        }

        // Tắt script này để dừng mọi logic (như di chuyển)
        this.enabled = false; 

        // Nếu bạn có Rigidbody2D, hãy tắt nó đi
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if(rb != null)
        {
            rb.simulated = false;
        }
        
        // (Tùy chọn) Chơi âm thanh hoặc animation chết ở đây
        // audioSource?.PlayOneShot(deathStompSound);
        
        // Cuối cùng, hủy GameObject
        // Bạn có thể thêm một khoảng trễ nhỏ (ví dụ 0.5f) nếu bạn có animation
        Destroy(gameObject);
    }
    public override void Hurt(int damage, Transform attackPosition)
    {
        base.Hurt(damage, attackPosition);
        audioSource?.PlayOneShot(hited);
    }
}
