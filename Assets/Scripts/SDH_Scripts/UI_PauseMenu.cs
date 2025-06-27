using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UI_PauseMenu : MonoBehaviour
{
    [SerializeField] private GameObject loadingScreen; // 로딩 화면 오브젝트
    [SerializeField] private GameObject pauseScreen; // 세팅 화면 오브젝트
    [SerializeField] private GameObject flyingGauge; // 게임 중 UI 오브젝트

    [Header("Slider")]
    [SerializeField] private Slider loadingSlider; // 로딩 진행 표시 슬라이더

    void Update()
    {
        // Tab 키를 눌렀을 때 일시정지 화면 활성화/비활성화
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            bool isPaused = pauseScreen.activeSelf;
            pauseScreen.SetActive(!isPaused);
            flyingGauge.SetActive(isPaused); // 반대로 flyingGauge 토글

            // 커서 상태 제어
            if (!isPaused)
            {
                // 일시정지 상태 진입
                Time.timeScale = 0f;
                Cursor.lockState = CursorLockMode.None; // 커서 자유
                Cursor.visible = true;
            }
            else
            {
                // 게임 재개
                Time.timeScale = 1f;
                Cursor.lockState = CursorLockMode.Locked; // 커서 잠금
                Cursor.visible = false;
            }
        }
    }

    public void LoadGame()
    {
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.Locked; // 커서 잠금
        Cursor.visible = false;
        pauseScreen.SetActive(false);
        flyingGauge.SetActive(true);
    }

    public void LoadLevelBtn(string levelToLoad)
    {
        Time.timeScale = 1f;
        pauseScreen.SetActive(false);
        loadingScreen.SetActive(true);
        loadingSlider.value = 0f;

        Debug.Log("Start loading: " + levelToLoad);
        StartCoroutine(LoadLevelASync(levelToLoad));
    }

    IEnumerator LoadLevelASync(string levelToLoad)
    {
        loadingSlider.value = 0f;

        AsyncOperation loadOperation = SceneManager.LoadSceneAsync(levelToLoad);
        loadOperation.allowSceneActivation = false;

        float fakeProgress = 0f;

        while (loadOperation.progress < 0.9f)
        {
            fakeProgress += Time.deltaTime * 0.05f;
            loadingSlider.value = Mathf.Min(fakeProgress, loadOperation.progress / 0.9f);
            yield return null;
        }

        while (loadingSlider.value < 1f)
        {
            loadingSlider.value += Time.deltaTime * 0.5f;
            yield return null;
        }

        yield return new WaitForSeconds(0.3f);

        loadOperation.allowSceneActivation = true;
    }

    public void ExitGame()
    {
        Application.Quit();
    }
}
