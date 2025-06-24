using UnityEngine;
using FMODUnity;
using FMOD.Studio;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance { get; private set; }
    [SerializeField]
    private string[] banksToLoad = { "SFX", "BGM", "Master", "Master.srings" };

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        LoadBanks(); // 모든 사운드가 담긴 Bank파일을 로드
    }
    
    private void LoadBanks()
    {
        foreach (var bank in banksToLoad)
        {
            RuntimeManager.LoadBank(bank, true);
        }
    }

}
