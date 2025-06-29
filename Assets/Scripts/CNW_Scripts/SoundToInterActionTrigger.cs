using UnityEngine;
using FMODUnity;

public class SoundToInterActionTrigger : MonoBehaviour
{
    [EventRef]
    public string interactSoundEvent;

    public void PlayInteractionSound()
    {
        if (!string.IsNullOrEmpty(interactSoundEvent))
        {
            RuntimeManager.PlayOneShot(interactSoundEvent, transform.position);
        }
    }
}