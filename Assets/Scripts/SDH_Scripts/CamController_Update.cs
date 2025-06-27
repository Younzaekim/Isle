using UnityEngine;

public class CamController_Update : MonoBehaviour
{
    [SerializeField] private PlayerController_Update player;
    [SerializeField] private float mouseSensitivity = 150f;
    [SerializeField] private Vector2 pitchClamp = new Vector2(-40f, 80f);

    private float yaw = 0f;
    private float pitch = 0f;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        float mouseX = Input.GetAxisRaw("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxisRaw("Mouse Y") * mouseSensitivity * Time.deltaTime;

        yaw += mouseX;
        pitch -= mouseY;

        // 플레이어 x로테이션 값 보정
        float playerPitch = 0f;
        if (player != null && player.isFlying)
        {
            playerPitch = player.transform.eulerAngles.x;
            if (playerPitch > 180f) playerPitch -= 360f;
        }

        // 플레이어가 비행 중이면 마우스 pitch와 플레이어 pitch를 합침 (가중치 조절 가능)
        if (player != null && player.isFlying)
        {
            pitch = Mathf.Lerp(pitch, playerPitch, 0.5f); // 0.5는 가중치 예시
        }

        pitch = Mathf.Clamp(pitch, pitchClamp.x, pitchClamp.y);

        transform.rotation = Quaternion.Euler(pitch, yaw, 0f);
    }

}
