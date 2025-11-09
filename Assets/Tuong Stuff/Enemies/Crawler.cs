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

    public override void Hurt(int damage, Transform attackPosition)
    {
        base.Hurt(damage, attackPosition);
        audioSource?.PlayOneShot(hited);
    }
}
