using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UI_MainMenu : MonoBehaviour
{
    [Header("Menu Screens")]
    [SerializeField] private GameObject loadingScreen; // 로딩 화면 오브젝트
    [SerializeField] private GameObject mainMenu; // 메인 메뉴 오브젝트
    [SerializeField] private GameObject settingsScreen; // 세팅 화면 오브젝트

    [Header("Slider")]
    [SerializeField] private Slider loadingSlider; // 로딩 진행 표시 슬라이더
    [SerializeField] Button fullscreenButton;
    [SerializeField] TextMeshProUGUI fullscreenButtonText; // Text를 쓴다면 Text로 변경
    [SerializeField] Slider volumeSlider;

    void Start()
    {
        // 저장된 값 불러오기
        bool isFullscreen = PlayerPrefs.GetInt("Fullscreen", 1) == 1;
        Screen.fullScreen = isFullscreen;
        UpdateFullscreenButtonText(isFullscreen);

        float savedVolume = PlayerPrefs.GetFloat("Volume", 1f);
        volumeSlider.value = savedVolume;
        AudioListener.volume = savedVolume;

        // 리스너 연결
        fullscreenButton.onClick.AddListener(ToggleFullscreen);
        volumeSlider.onValueChanged.AddListener(OnVolumeChanged);
    }

    public void ToggleFullscreen()
    {
        bool newFullscreenState = !Screen.fullScreen;
        Screen.fullScreen = newFullscreenState;

        PlayerPrefs.SetInt("Fullscreen", newFullscreenState ? 1 : 0);
        UpdateFullscreenButtonText(newFullscreenState);
    }

    void UpdateFullscreenButtonText(bool isFullscreen)
    {
        fullscreenButtonText.text = isFullscreen ? "Window" : "FullScreen";
    }

    void OnVolumeChanged(float value)
    {
        AudioListener.volume = value;
        PlayerPrefs.SetFloat("Volume", value);
    }

    // 씬 로드를 위한 버튼 클릭 시 호출되는 메서드
    public void LoadSettingsBtn()
    {
        //mainMenu.SetActive(false); // 메인 메뉴 비활성화
        settingsScreen.SetActive(true); // 세팅 화면 활성화
    }

    // 씬 로드를 위한 버튼 클릭 시 호출되는 메서드
    public void LoadMainMenuBtn()
    {
        settingsScreen.SetActive(false); // 세팅 화면 활성화
        //mainMenu.SetActive(true); // 메인 메뉴 비활성화
    }

    // 씬 로드를 위한 버튼 클릭 시 호출되는 메서드
    public void LoadLevelBtn(string levelToLoad)
    {
        mainMenu.SetActive(false); // 메인 메뉴 비활성화
        loadingScreen.SetActive(true); // 로딩 화면 활성화
        loadingSlider.value = 0f; // 초기화

        Debug.Log("Start loading: " + levelToLoad); // 확인용
        StartCoroutine(LoadLevelASync(levelToLoad)); // 비동기 씬 로드 코루틴 시작
    }

    // 씬을 비동기적으로 로드하고 로딩 슬라이더를 갱신하는 코루틴
    IEnumerator LoadLevelASync(string levelToLoad)
    {
        loadingSlider.value = 0f;

        AsyncOperation loadOperation = SceneManager.LoadSceneAsync(levelToLoad);
        loadOperation.allowSceneActivation = false;

        float fakeProgress = 0f;

        // 실제 로딩 progress가 0.9까지 도달할 동안, 가짜 진행도 증가
        while (loadOperation.progress < 0.9f)
        {
            fakeProgress += Time.deltaTime * 0.05f; // 1초에 5% 증가
            loadingSlider.value = Mathf.Min(fakeProgress, loadOperation.progress / 0.9f);
            yield return null;
        }

        // 실제 로딩 완료 후, 100%까지 천천히 증가
        while (loadingSlider.value < 1f)
        {
            loadingSlider.value += Time.deltaTime * 0.5f;
            yield return null;
        }

        yield return new WaitForSeconds(0.3f); // 짧은 딜레이

        loadOperation.allowSceneActivation = true; // 씬 전환
    }

    public void ExitGame()
    {
        Application.Quit();
    }
}
