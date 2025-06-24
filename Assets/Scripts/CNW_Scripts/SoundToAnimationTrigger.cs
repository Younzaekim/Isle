using UnityEngine;

public class SoundToAnimationTrigger : MonoBehaviour
{
    private SurfaceSensor surfaceSensor;
    private FootstepPlayer footstepPlayer;

    private void Awake()
    {
        surfaceSensor = GetComponent<SurfaceSensor>();
        footstepPlayer = GetComponentInChildren<FootstepPlayer>();
    }

    public void AnimationEvent_Footstep()
    {
        footstepPlayer.PlayFootstep(surfaceSensor.CurrentSurface);
    }

    public void AnimationEvent_JumpStart()
    {
        footstepPlayer.PlayJumpStart(surfaceSensor.CurrentSurface);
    }

    public void AnimationEvent_JumpLand()
    {
        footstepPlayer.PlayJumpLand(surfaceSensor.CurrentSurface);
    }
}
