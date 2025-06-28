using UnityEngine;
using FMODUnity;
using FMOD.Studio;

public enum SurfaceType
{
    Leave, Rock, Sand, Water, Wood
}

//이 프리팹의 포지션에서 소리가 나온다. player에 딱 붙게끔 0,0,0 위치로 항상 고정해야함
public class PlayerSFX : MonoBehaviour
{
    [EventRef] public string footstepEvent;
    [EventRef] public string jumpstartEvent;
    [EventRef] public string jumplandEvent;
    [EventRef] public string flyLoopEvent;
    [EventRef] public string interActionEvent;

    private EventInstance flyLoopInstance;
    private bool isFlyLoopPlaying = false;

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

    //Fly 루프

    public void PlayFlyLoop()
    {
        if (isFlyLoopPlaying || string.IsNullOrEmpty(flyLoopEvent)) return;

        flyLoopInstance = RuntimeManager.CreateInstance(flyLoopEvent);
        flyLoopInstance.set3DAttributes(RuntimeUtils.To3DAttributes(transform.position));
        flyLoopInstance.start();
        isFlyLoopPlaying = true;
    }

    public void StopFlyLoop()
    {
        if (!isFlyLoopPlaying) return;

        flyLoopInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        flyLoopInstance.release();
        isFlyLoopPlaying = false;
    }

    public void UpdateFlyLoopPosition()
    {
        if (isFlyLoopPlaying)
        {
            flyLoopInstance.set3DAttributes(RuntimeUtils.To3DAttributes(transform.position));
        }
    }
}
