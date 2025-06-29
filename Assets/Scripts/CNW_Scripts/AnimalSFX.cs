using UnityEngine;
using FMODUnity;
using FMOD.Studio;

public class AnimalSFX : MonoBehaviour
{
    [EventRef] public string idleLoopEvent;
    [EventRef] public string alertEvent;

    private EventInstance idleLoopInstance;

    private void Start()
    {
        // 평상시 울음소리 루프 재생
        if (!string.IsNullOrEmpty(idleLoopEvent))
        {
            idleLoopInstance = RuntimeManager.CreateInstance(idleLoopEvent);
            idleLoopInstance.set3DAttributes(RuntimeUtils.To3DAttributes(transform.position));
            idleLoopInstance.start();
        }
    }

    // 외부에서 호출 (상호작용 시 반응 사운드 재생)
    public void PlayAlert()
    {
        if (!string.IsNullOrEmpty(alertEvent))
        {
            RuntimeManager.PlayOneShot(alertEvent, transform.position);
        }
    }

    private void Update()
    {
        // 계속 위치 갱신 (움직이는 동물이라면)
        if (idleLoopInstance.isValid())
        {
            idleLoopInstance.set3DAttributes(RuntimeUtils.To3DAttributes(transform.position));
        }
    }

    private void OnDestroy()
    {
        // 정리
        if (idleLoopInstance.isValid())
        {
            idleLoopInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
            idleLoopInstance.release();
        }
    }
}
