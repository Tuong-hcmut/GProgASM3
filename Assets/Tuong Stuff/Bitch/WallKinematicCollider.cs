using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WallKinematicCollider : MonoBehaviour
{
    PlayerMovement movement;

    private void Awake()
    {
        movement = FindFirstObjectByType<PlayerMovement>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            movement.SetIsSliding(true);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            movement.SetIsSliding(false);
        }
    }
}