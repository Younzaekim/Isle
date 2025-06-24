using UnityEngine;
using FMODUnity;
using FMOD.Studio;
public enum SurfaceType
{
    Leave, Rock, Sand, Water, Wood
}

public class FootstepPlayer : MonoBehaviour
{
    [EventRef] public string footstepEvent;
    [EventRef] public string jumpstartEvent;
    [EventRef] public string jumplandEvent;

    public void PlayFootstep(SurfaceType type)
    {
        PlayEvent(footstepEvent, type);
    }

    public void PlayJumpStart(SurfaceType type)
    {
        PlayEvent(jumpstartEvent, type);
    }

    public void PlayJumpLand(SurfaceType type)
    {
        PlayEvent(jumplandEvent, type);
    }

    private void PlayEvent(string eventPath, SurfaceType type)
    {
        if (string.IsNullOrEmpty(eventPath)) return;

        EventInstance instance = RuntimeManager.CreateInstance(eventPath);
        instance.setParameterByName("SurfaceType", (float)type);
        instance.set3DAttributes(RuntimeUtils.To3DAttributes(transform.position));
        instance.start();
        instance.release();
    }
}
