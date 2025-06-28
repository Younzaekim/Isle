using UnityEngine;
using UnityEngine.UI;

public class UI_FlyingGauge : MonoBehaviour
{
    [SerializeField] private Image gauge; // 게이지 UI
    [SerializeField] private PlayerController_Update player; // 인스펙터에서 할당할 플레이어 컨트롤러

    void Update()
    {
        // 플레이어가 할당되지 않았으면 처리하지 않음
        if (player == null)
        {
            Debug.LogWarning("UI_FlyingGauge: PlayerController_Test가 할당되지 않았습니다.");
            return;
        }

        // 매 프레임마다 게이지 업데이트
        HandleBar();
    }

    private void HandleBar()
    {
        // 비행 시간 게이지를 현재 남은 시간 비율로 설정
        // 남은 시간 비율 = (총 비행 시간 - 경과 시간) / 총 비행 시간
        gauge.fillAmount = Mathf.Clamp01((player.flyingTime - player.flyingTimer) / player.flyingTime);
    }
}
