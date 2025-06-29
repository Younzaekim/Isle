using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// 일시정지 메뉴 UI를 관리하는 클래스
public class UI_PauseMenu : MonoBehaviour
{
    [Header("Pause")]
    [SerializeField] private GameObject pauseScreen; // 일시정지 화면 오브젝트

    [Header("Loading")]
    [SerializeField] private GameObject loadingScreen; // 로딩 화면 오브젝트
    [SerializeField] private Slider loadingSlider; // 로딩 진행도 표시 슬라이더

    [Header("InGame UI")]
    [SerializeField] private GameObject flyingGauge; // 게임 중 표시되는 UI (예: 게이지 등)

    private void Update()
    {
        // Tab 키를 눌렀을 때 일시정지 토글 처리
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            bool isPaused = pauseScreen.activeSelf; // 현재 일시정지 상태 확인
            pauseScreen.SetActive(!isPaused); // 일시정지 화면 토글
            flyingGauge.SetActive(isPaused); // flyingGauge는 반대로 토글

            // 커서 상태 및 시간 정지/재개 설정
            if (!isPaused)
            {
                // 일시정지 상태 진입
                Time.timeScale = 0f; // 시간 정지
                Cursor.lockState = CursorLockMode.None; // 커서 자유 이동
                Cursor.visible = true;
            }
            else
            {
                // 게임 재개
                Time.timeScale = 1f; // 시간 재개
                Cursor.lockState = CursorLockMode.Locked; // 커서 잠금
                Cursor.visible = false;
            }
        }
    }

    // 게임을 다시 시작할 때 호출됨 (일시정지 해제)
    public void LoadGame()
    {
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        pauseScreen.SetActive(false); // 일시정지 화면 비활성화
        flyingGauge.SetActive(true); // UI 재활성화
    }

    // 선택한 씬으로 로딩할 때 호출됨
    public void LoadLevelBtn(string levelToLoad)
    {
        Time.timeScale = 1f; // 시간 정지 해제
        pauseScreen.SetActive(false); // 일시정지 화면 비활성화
        loadingScreen.SetActive(true); // 로딩 화면 표시
        loadingSlider.value = 0f; // 로딩 슬라이더 초기화

        Debug.Log("Start loading: " + levelToLoad);
        StartCoroutine(LoadLevelASync(levelToLoad)); // 비동기 씬 로드 시작
    }

    // 비동기적으로 씬을 로드하고 로딩 슬라이더를 갱신하는 코루틴
    private IEnumerator LoadLevelASync(string levelToLoad)
    {
        loadingSlider.value = 0f;

        AsyncOperation loadOperation = SceneManager.LoadSceneAsync(levelToLoad);
        loadOperation.allowSceneActivation = false; // 로딩 완료 후 자동 전환 방지

        float fakeProgress = 0f;

        // 실제 progress가 0.9에 도달할 때까지 가짜 진행도 증가
        while (loadOperation.progress < 0.9f)
        {
            fakeProgress += Time.deltaTime * 0.05f;
            loadingSlider.value = Mathf.Min(fakeProgress, loadOperation.progress / 0.9f);
            yield return null;
        }

        // 나머지 10%를 부드럽게 채움
        while (loadingSlider.value < 1f)
        {
            loadingSlider.value += Time.deltaTime * 0.5f;
            yield return null;
        }

        yield return new WaitForSeconds(0.3f); // 짧은 딜레이 추가

        loadOperation.allowSceneActivation = true; // 씬 전환 허용
    }

    // 게임 종료 버튼 클릭 시 호출
    public void ExitGame()
    {
        Application.Quit(); // 게임 종료
    }
}
