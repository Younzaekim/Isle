using UnityEngine;

[RequireComponent(typeof(PlayerController_Update))]
public class PlayerMovementSoundTrigger : MonoBehaviour
{
    private PlayerController_Update player;
    private SurfaceSensor surfaceSensor;
    private PlayerSFX playerSFX;

    private bool wasGrounded = true;
    private bool hasJumped = false;

    private void Awake()
    {
        player = GetComponent<PlayerController_Update>();
        surfaceSensor = GetComponentInChildren<SurfaceSensor>();
        playerSFX = GetComponentInChildren<PlayerSFX>();
    }

    private void Update()
    {
        HandleJumpLandSound();
        HandleFlyLoopSound();
        InteractSound();
    }

    public void AnimationEvent_Footstep()
    {
        playerSFX.PlayFootstep(surfaceSensor.CurrentSurface);
    }

    public void AnimationEvent_JumpStart()
    {
        if (hasJumped) return;

        playerSFX.PlayJumpStart(surfaceSensor.CurrentSurface);
        hasJumped = true;
    }

    private void HandleJumpLandSound()
    {
        bool isGrounded = player.isGrounded;

        if (isGrounded && !wasGrounded)
        {
            playerSFX.PlayJumpLand(surfaceSensor.CurrentSurface);
            hasJumped = false;
        }

        wasGrounded = isGrounded;
    }

    private void HandleFlyLoopSound()
    {
        if (player.isFlying)
        {
            playerSFX.PlayFlyLoop();
        }
        else
        {
            playerSFX.StopFlyLoop();
        }

        // 위치 갱신
        playerSFX.UpdateFlyLoopPosition();
    }

    private void InteractSound()
    {
        if (Input.GetKeyDown(KeyCode.LeftShift) || (Input.GetKeyDown(KeyCode.RightShift)))
            playerSFX.PlayInteractSound();
    }
}
