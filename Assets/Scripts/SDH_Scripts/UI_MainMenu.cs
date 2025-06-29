using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UI_MainMenu : MonoBehaviour
{
    [Header("Main Menu")]
    [SerializeField] private GameObject mainMenuScreen; // 메인 메뉴 오브젝트

    [Header("Settings")]
    [SerializeField] private GameObject settingsScreen; // 세팅 화면 오브젝트
    [SerializeField] private Button fullscreenButton; // 전체화면 토글 버튼
    [SerializeField] private TextMeshProUGUI fullscreenButtonText; // 전체화면 버튼 텍스트
    [SerializeField] private Slider volumeSlider; // 볼륨 조절 슬라이더

    [Header("Loading")]
    [SerializeField] private GameObject loadingScreen; // 로딩 화면 오브젝트
    [SerializeField] private Slider loadingSlider; // 로딩 진행 표시 슬라이더

    private void Start()
    {
        // 저장된 전체화면 설정 불러오기 (없으면 기본값 true)
        bool isFullscreen = PlayerPrefs.GetInt("Fullscreen", 1) == 1;
        Screen.fullScreen = isFullscreen; // 화면 전체화면 여부 적용
        UpdateFullscreenButtonText(isFullscreen); // 버튼 텍스트 갱신

        // 저장된 볼륨 값 불러오기 (없으면 기본값 1f)
        float savedVolume = PlayerPrefs.GetFloat("Volume", 1f);
        volumeSlider.value = savedVolume; // 슬라이더 위치 설정
        AudioListener.volume = savedVolume; // 오디오 볼륨 적용

        // 버튼과 슬라이더 이벤트 리스너 등록
        fullscreenButton.onClick.AddListener(ToggleFullscreen);
        volumeSlider.onValueChanged.AddListener(OnVolumeChanged);
    }

    // 전체화면 모드 토글 함수 (버튼 클릭 시 호출됨)
    public void ToggleFullscreen()
    {
        bool newFullscreenState = !Screen.fullScreen; // 현재 상태 반전
        Screen.fullScreen = newFullscreenState; // 전체화면 상태 변경

        // 변경된 상태를 저장
        PlayerPrefs.SetInt("Fullscreen", newFullscreenState ? 1 : 0);
        UpdateFullscreenButtonText(newFullscreenState); // 버튼 텍스트 갱신
    }

    // 전체화면 상태에 따라 버튼 텍스트 갱신
    private void UpdateFullscreenButtonText(bool isFullscreen)
    {
        fullscreenButtonText.text = isFullscreen ? "Window" : "FullScreen";
    }

    // 볼륨 슬라이더 값 변경 시 호출되는 함수
    private void OnVolumeChanged(float value)
    {
        AudioListener.volume = value; // 오디오 볼륨 적용
        PlayerPrefs.SetFloat("Volume", value); // 볼륨 값 저장
    }

    // 설정 화면으로 전환 (설정 버튼 클릭 시 호출)
    public void LoadSettingsBtn()
    {
        //mainMenu.SetActive(false); // 메인 메뉴 비활성화 (주석 처리됨)
        settingsScreen.SetActive(true); // 설정 화면 활성화
    }

    // 메인 메뉴 화면으로 전환 (뒤로가기 버튼 등에서 호출)
    public void LoadMainMenuBtn()
    {
        settingsScreen.SetActive(false); // 설정 화면 비활성화
        //mainMenu.SetActive(true); // 메인 메뉴 활성화 (주석 처리됨)
    }

    // 특정 레벨 씬 로드 시작 (레벨 선택 버튼 클릭 시 호출)
    public void LoadLevelBtn(string levelToLoad)
    {
        mainMenuScreen.SetActive(false); // 메인 메뉴 화면 비활성화
        loadingScreen.SetActive(true); // 로딩 화면 활성화
        loadingSlider.value = 0f; // 로딩 슬라이더 초기화

        Debug.Log("Start loading: " + levelToLoad); // 디버그 로그 출력
        StartCoroutine(LoadLevelASync(levelToLoad)); // 비동기 씬 로드 코루틴 시작
    }

    // 씬을 비동기적으로 로드하면서 로딩 슬라이더를 업데이트하는 코루틴
    private IEnumerator LoadLevelASync(string levelToLoad)
    {
        loadingSlider.value = 0f; // 슬라이더 초기화

        AsyncOperation loadOperation = SceneManager.LoadSceneAsync(levelToLoad);
        loadOperation.allowSceneActivation = false; // 씬 자동 전환 비활성화

        float fakeProgress = 0f; // 가짜 진행도 변수

        // 실제 로딩 진행률이 90% 미만일 때
        while (loadOperation.progress < 0.9f)
        {
            fakeProgress += Time.deltaTime * 0.05f; // 가짜 진행도 천천히 증가 (1초에 5%)
            // 가짜 진행도와 실제 로딩 진행도를 비교해 더 작은 쪽을 슬라이더 값으로 설정
            loadingSlider.value = Mathf.Min(fakeProgress, loadOperation.progress / 0.9f);
            yield return null; // 다음 프레임까지 대기
        }

        // 실제 로딩이 완료되고 난 뒤, 슬라이더를 100%까지 천천히 증가시킴
        while (loadingSlider.value < 1f)
        {
            loadingSlider.value += Time.deltaTime * 0.5f; // 빠르게 증가 (1초에 50%)
            yield return null; // 다음 프레임까지 대기
        }

        yield return new WaitForSeconds(0.3f); // 짧은 지연

        loadOperation.allowSceneActivation = true; // 씬 전환 허용
    }

    // 게임 종료 함수 (종료 버튼 클릭 시 호출)
    public void ExitGame()
    {
        Application.Quit(); // 애플리케이션 종료
    }
}
