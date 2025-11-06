/// <summary>
/// PlayerController — Handles communication between input, movement, entity lifecycle, etc.
/// Responsibilities:
///  - Read input from PlayerInputHandler and forward it to PlayerMovement (via HandleInput) or other components
///  - Manage control gating (e.g. disable controls on death)
/// 
/// Note: PlayerController must not contain any gameplay logic and only act as a coordinator.
/// </summary>
using UnityEngine;

[RequireComponent(typeof(PlayerInputHandler))]
[RequireComponent(typeof(PlayerMovement))]
[RequireComponent(typeof(PlayerAttack))]
[RequireComponent(typeof(AnimationController))]
public class PlayerController : MonoBehaviour
{
    PlayerInputHandler inputHandler;
    PlayerMovement movement;
    PlayerAttack attack;
    AnimationController anim;
    BaseEntity baseEntity;

    void Awake()
    {
        inputHandler = GetComponent<PlayerInputHandler>();
        movement = GetComponent<PlayerMovement>();
        attack = GetComponent<PlayerAttack>();
        anim = GetComponent<AnimationController>();
        baseEntity = GetComponent<BaseEntity>();
    }

    // Wrapper methods from original CharacterController2D for external calls (used by animation events, other systems):
    /* public void FirstLand()
     {
         movement.StopInput();
         baseEntity.GetEffecter()?.DoEffect(CharacterEffect.EffectType.BurstRocks, true);
     }*/

    public void HardLand() => movement.StopInput();
    //  public void StartShake() => ProCamera2DShake.Instance.Shake(ProCamera2DShake.Instance.ShakePresets[0]);
    //  public void StopShake() => ProCamera2DShake.Instance.StopShaking();
    /* public void PlayHitParticles()
     {
         baseEntity.GetEffecter()?.DoEffect(CharacterEffect.EffectType.HitLeft, true);
         baseEntity.GetEffecter()?.DoEffect(CharacterEffect.EffectType.HitRight, true);
     }
     public void PlayAshParticles()
     {
         baseEntity.GetEffecter()?.DoEffect(CharacterEffect.EffectType.AshLeft, true);
         baseEntity.GetEffecter()?.DoEffect(CharacterEffect.EffectType.AshRight, true);
     }
     public void PlayShadeParticle() => baseEntity.GetEffecter()?.DoEffect(CharacterEffect.EffectType.Shade, true);*/
    public void PlayRespawnAnimation() => anim.SetTrigger(anim.respawnTrigger);
    public bool GetIsOnGround() => movement.GetIsOnGround();
    public void PlayMusicAudioClip(AudioClip audioClip) => baseEntity.GetMusicPlayer()?.PlayOneShot(audioClip);
    //public void ResetFallDistance() => movement.ResetFallDistance();
    public void SlideWall_ResetJumpCount() => movement.SlideWall_ResetJumpCount();
    public void SetIsSliding(bool state) => movement.SetIsSliding(state);
    public void SetIsOnGrounded(bool state) => movement.SetIsOnGrounded(state);
}