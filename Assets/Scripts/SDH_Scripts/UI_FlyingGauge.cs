using UnityEngine;
using UnityEngine.UI;

public class UI_FlyingGauge : MonoBehaviour
{
    [SerializeField] private float fillAmount;
    [SerializeField] private Image content;
    private PlayerController_Test player;

    private void Start()
    {
        player = GetComponentInParent<PlayerController_Test>();
    }

    private void Update()
    {
        HandleBar();
    }

    private void HandleBar()
    {
        // 남은 시간 비율 = (남은 시간) / (총 시간)
        content.fillAmount = Mathf.Clamp01((player.flyingTime - player.flyingTimer) / player.flyingTime);
    }
}
