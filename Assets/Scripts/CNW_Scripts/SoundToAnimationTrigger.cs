using UnityEngine;

public class SoundToAnimationTrigger : MonoBehaviour
{
    private SurfaceSensor surfaceSensor;
    private FootstepPlayer footstepPlayer;

    private void Awake()
    {
        surfaceSensor = GetComponentInChildren<SurfaceSensor>();
        Debug.Log(surfaceSensor);
        footstepPlayer = GetComponentInChildren<FootstepPlayer>();
        Debug.Log(footstepPlayer);
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
