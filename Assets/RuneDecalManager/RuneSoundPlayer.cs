using UnityEngine;

public class RuneSoundPlayer : MonoBehaviour
{
    [Header("사운드 클립 할당")]
    [SerializeField] private AudioClip stage1Sound;
    [SerializeField] private AudioClip stage2Sound;
    [SerializeField] private AudioClip stage3Sound;
    [SerializeField] private AudioClip sequenceCompleteSound;
    [SerializeField] private AudioClip resetSound;

    private AudioSource audioSource; 

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        // RuneSequenceManager의 이벤트에 구독
        RuneSequenceManager.OnStage1Start += PlayStage1Sound;
        RuneSequenceManager.OnStage2Start += PlayStage2Sound;
        RuneSequenceManager.OnStage3Start += PlayStage3Sound;
        RuneSequenceManager.OnSequenceCompleted += PlaySequenceCompleteSound;
        RuneSequenceManager.OnResetStart += PlayResetSound;
    }

    void OnDestroy()
    {
        // 오브젝트가 파괴될 때 구독을 해제
        RuneSequenceManager.OnStage1Start -= PlayStage1Sound;
        RuneSequenceManager.OnStage2Start -= PlayStage2Sound;
        RuneSequenceManager.OnStage3Start -= PlayStage3Sound;
        RuneSequenceManager.OnSequenceCompleted -= PlaySequenceCompleteSound;
        RuneSequenceManager.OnResetStart -= PlayResetSound;
    }

    private void PlayStage1Sound()
    {
        if (stage1Sound != null && audioSource != null)
        {
            audioSource.PlayOneShot(stage1Sound);
            Debug.Log("룬 시퀀스 1단계 사운드");
        }
    }

    private void PlayStage2Sound()
    {
        if (stage2Sound != null && audioSource != null)
        {
            audioSource.PlayOneShot(stage2Sound);
            Debug.Log("룬 시퀀스 2단계 사운드");
        }
    }

    private void PlayStage3Sound()
    {
        if (stage3Sound != null && audioSource != null)
        {
            audioSource.PlayOneShot(stage3Sound);
            Debug.Log("룬 시퀀스 3단계 사운드");
        }
    }

    private void PlaySequenceCompleteSound()
    {
        if (sequenceCompleteSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(sequenceCompleteSound);
            Debug.Log("룬 시퀀스 완료 사운드");
        }
    }

    private void PlayResetSound()
    {
        if (resetSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(resetSound);
            Debug.Log("룬 시퀀스 리셋 사운드");
        }
    }
}