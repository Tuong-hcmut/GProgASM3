/// <summary>
/// PlayerController — Handles communication between input, movement, entity lifecycle, etc.
/// Responsibilities:
///  - Read input from PlayerInputHandler and forward it to PlayerMovement (via HandleInput) or other components
///  - Manage control gating (e.g. disable controls on death)
/// 
/// Note: PlayerController must not contain any gameplay logic and only act as a coordinator.
/// </summary>
using UnityEngine;

[RequireComponent(typeof(AnimationController))]
public class Enemy : MonoBehaviour
{
    AnimationController anim;
    BaseEntity baseEntity;

    void Awake()
    {
        anim = GetComponent<AnimationController>();
        baseEntity = GetComponent<BaseEntity>();
    }
}