using UnityEngine;
using FMODUnity;
using FMOD.Studio;

public class PlayerAreaSoundTrigger : MonoBehaviour
{
    [EventRef] public string bgmEvent;
    [EventRef] public string ambientEvent;

    private EventInstance bgmInstance;
    private EventInstance ambientInstance;
    private bool hasEntered = false;

    private void OnTriggerEnter(Collider other)
    {
        if (!hasEntered && other.CompareTag("Player"))
        {
            hasEntered = true;

            // BGM (2D)
            if (!string.IsNullOrEmpty(bgmEvent))
            {
                bgmInstance = RuntimeManager.CreateInstance(bgmEvent);
                bgmInstance.start();
            }

            // Ambient (2D)
            if (!string.IsNullOrEmpty(ambientEvent))
            {
                ambientInstance = RuntimeManager.CreateInstance(ambientEvent);
                ambientInstance.start();
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (hasEntered && other.CompareTag("Player"))
        {
            hasEntered = false;

            // 종료 및 해제
            bgmInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
            bgmInstance.release();

            ambientInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
            ambientInstance.release();
        }
    }
}
