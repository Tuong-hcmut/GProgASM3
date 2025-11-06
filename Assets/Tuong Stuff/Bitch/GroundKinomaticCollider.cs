using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroundKinomaticCollider : MonoBehaviour
{
    private PlayerMovement movement;

    private void Awake()
    {
        movement = GetComponentInParent<PlayerMovement>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            movement.SetIsOnGrounded(true);
            movement.Ground_ResetJumpCount();
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            movement.SetIsOnGrounded(false);
        }
    }
}