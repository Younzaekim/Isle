using UnityEngine;
using FMODUnity;
using FMOD.Studio;

[RequireComponent(typeof(PlayerController))]
public class FlySoundController : MonoBehaviour
{
    [EventRef] public string flyLoopEvent;

    private PlayerController_Test player; // 컨트롤러 정보
    private EventInstance flyLoopInstance;
    private bool isFlyLoopPlaying = false;

    private void Awake()
    {
        player = GetComponent<PlayerController_Test>(); //컨트롤러 정보
    }

    private void Update()
    {
        if (player.isFlying && !isFlyLoopPlaying)
        {
            StartFlyLoop();
        }
        else if (!player.isFlying && isFlyLoopPlaying)
        {
            StopFlyLoop();
        }
    }

    private void StartFlyLoop()
    {
        if (string.IsNullOrEmpty(flyLoopEvent)) return;

        flyLoopInstance = RuntimeManager.CreateInstance(flyLoopEvent);
        flyLoopInstance.set3DAttributes(RuntimeUtils.To3DAttributes(transform.position));
        flyLoopInstance.start();
        isFlyLoopPlaying = true;
    }

    private void StopFlyLoop()
    {
        flyLoopInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        flyLoopInstance.release();
        isFlyLoopPlaying = false;
    }
}
