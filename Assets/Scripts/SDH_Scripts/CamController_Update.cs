using UnityEngine;

public class CamController_Update : MonoBehaviour
{
    // 따라다닐 플레이어 객체
    [SerializeField] private PlayerController_Update player;

    // 마우스 감도
    [SerializeField] private float mouseSensitivity = 150f;

    // 위아래 회전 각도 제한 (pitch)
    [SerializeField] private Vector2 pitchClamp = new Vector2(-20f, 80f);

    // 좌우 회전 각도 (yaw)
    private float yaw = 0f;

    // 위아래 회전 각도 (pitch)
    private float pitch = 0f;

    private void Start()
    {
        // 커서를 화면 중앙에 고정하고 숨김
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void Update()
    {
        // 마우스 이동 값 계산
        float mouseX = Input.GetAxisRaw("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxisRaw("Mouse Y") * mouseSensitivity * Time.deltaTime;

        // 좌우(yaw)는 마우스 X축 입력만큼 누적
        yaw += mouseX;

        // 위아래(pitch)는 마우스 Y축 입력만큼 감소 (마우스를 위로 올리면 아래로 내려가도록)
        pitch -= mouseY;

        // 플레이어가 공중에 있는 경우
        if (player != null && player.isAirborne)
        {
            // 플레이어의 현재 x축 회전 각도 추출
            Vector3 angles = player.transform.eulerAngles;

            // 360도를 기준으로 음수 각도로 변환 (180 이상이면 -180 ~ 180 범위로 조정)
            float playerPitch = angles.x > 180f ? angles.x - 360f : angles.x;

            // 현재 pitch를 제한 범위 내로 보정
            float pitchOffset = Mathf.Clamp(pitch, pitchClamp.x, pitchClamp.y);

            // pitch를 플레이어 기준 각도 + 오프셋으로 설정
            pitch = playerPitch + pitchOffset;
        }
        else
        {
            // 평소에는 pitch만 제한 범위로 보정
            pitch = Mathf.Clamp(pitch, pitchClamp.x, pitchClamp.y);
        }

        // 카메라 회전 적용
        transform.rotation = Quaternion.Euler(pitch, yaw, 0f);
    }
}
