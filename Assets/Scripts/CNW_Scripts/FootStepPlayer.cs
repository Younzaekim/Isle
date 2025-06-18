using UnityEngine;
using FMODUnity;
using FMOD.Studio;

public enum SurfaceType
{
    Leave = 0,
    Rock = 1,
    Sand = 2,
    Water = 3,
    Wood = 4
}

public class FootstepPlayer : MonoBehaviour
{
    [EventRef]
    public string footstepEvent = "event:/SFX/Player/Footstep/Walk";

    public void PlayFootstep(SurfaceType type)
    {
        Debug.Log("재생 시도: " + type);
        EventInstance instance = RuntimeManager.CreateInstance(footstepEvent);

        instance.setParameterByName("SurfaceType", (float)type);
        instance.set3DAttributes(RuntimeUtils.To3DAttributes(transform.position));
        instance.start();
        instance.release();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
            GetComponent<FootstepPlayer>().PlayFootstep(SurfaceType.Leave);

        if (Input.GetKeyDown(KeyCode.Alpha2))
            GetComponent<FootstepPlayer>().PlayFootstep(SurfaceType.Rock);

        if (Input.GetKeyDown(KeyCode.Alpha3))
            GetComponent<FootstepPlayer>().PlayFootstep(SurfaceType.Sand);

        if (Input.GetKeyDown(KeyCode.Alpha4))
            GetComponent<FootstepPlayer>().PlayFootstep(SurfaceType.Water);
            
        if (Input.GetKeyDown(KeyCode.Alpha5))
            GetComponent<FootstepPlayer>().PlayFootstep(SurfaceType.Wood);
    }
}
