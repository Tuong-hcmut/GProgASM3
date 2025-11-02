using UnityEngine;

public class FallingBehaviour : StateMachineBehaviour
{
    float lastPositionY, fallDistance;
    PlayerAudio audioPlayer;
    PlayerMovement movement;

    private void Awake()
    {
        audioPlayer = FindFirstObjectByType<PlayerAudio>();
        movement = FindFirstObjectByType<PlayerMovement>();
    }

    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        fallDistance = 0;
        animator.SetFloat("FallDistance", fallDistance);

        audioPlayer.Play(PlayerAudio.AudioType.Falling, true);
    }

    // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (lastPositionY > movement.transform.position.y)
        {
            fallDistance += lastPositionY - movement.transform.position.y;
        }
        lastPositionY = movement.transform.position.y;
        animator.SetFloat("FallDistance", fallDistance);
    }

    // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        audioPlayer.Play(PlayerAudio.AudioType.Falling, false);
    }

    // OnStateMove is called right after Animator.OnAnimatorMove()
    //override public void OnStateMove(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    // Implement code that processes and affects root motion
    //}

    // OnStateIK is called right after Animator.OnAnimatorIK()
    //override public void OnStateIK(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    // Implement code that sets up animation IK (inverse kinematics)
    //}

    public void ResetAllParams()
    {
        lastPositionY = movement.transform.position.y;
        fallDistance = 0;
    }
}
