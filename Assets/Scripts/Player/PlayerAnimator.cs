using UnityEngine;


[RequireComponent(typeof(Animator))]
public class PlayerAnimator : MonoBehaviour
{
    private Animator animator;

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    public void PlayAnimation(string animationName)
    {
        animator.Play(animationName);
    }

    #region AnimationEvent
    
    public void OnDashOver()
    {
        PlayerController.Instance.ChangeState(PlayerState.Idle);
    }

    #endregion
}