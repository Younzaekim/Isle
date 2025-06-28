using UnityEngine;

[RequireComponent(typeof(PlayerController))]
public class SoundToAnimationTrigger : MonoBehaviour
{
    private SurfaceSensor surfaceSensor;
    private FootstepPlayer footstepPlayer;
    private PlayerController playerController;

    private bool wasGrounded = true;
    private bool hasJumped = false;

    private void Awake()
    {
        surfaceSensor = GetComponentInChildren<SurfaceSensor>();
        footstepPlayer = GetComponentInChildren<FootstepPlayer>();
        playerController = GetComponent<PlayerController>();
    }

    private void Update()
    {
        AnimationEvent_JumpLand();
    }

    public void AnimationEvent_Footstep()
    {
        footstepPlayer.PlayFootstep(surfaceSensor.CurrentSurface);
    }

    public void AnimationEvent_JumpStart()
    {
        if (hasJumped) return;

        footstepPlayer.PlayJumpStart(surfaceSensor.CurrentSurface);
        hasJumped = true;
    }

    public void AnimationEvent_JumpLand()
    {
        bool isGrounded = playerController.isGrounded;

        // 착지: 공중에서 지면으로 바뀌는 순간
        if (isGrounded && !wasGrounded)
        {
            footstepPlayer.PlayJumpLand(surfaceSensor.CurrentSurface);
            hasJumped = false;
        }

        wasGrounded = isGrounded;
    }
}
