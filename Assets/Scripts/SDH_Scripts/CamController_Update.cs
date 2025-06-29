using UnityEngine;

public class CamController_Update : MonoBehaviour
{
    [SerializeField] private PlayerController_Update player;
    [SerializeField] private float mouseSensitivity = 150f;
    [SerializeField] private Vector2 pitchClamp = new Vector2(-20f, 80f);

    private float yaw = 0f;
    private float pitch = 0f;

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void Update()
    {
        float mouseX = Input.GetAxisRaw("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxisRaw("Mouse Y") * mouseSensitivity * Time.deltaTime;

        yaw += mouseX;
        pitch -= mouseY;

        if (player != null && player.isAirborne)
        {
            Vector3 angles = player.transform.eulerAngles;

            float playerPitch = angles.x > 180f ? angles.x - 360f : angles.x;

            float pitchOffset = Mathf.Clamp(pitch, pitchClamp.x, pitchClamp.y);
            pitch = playerPitch + pitchOffset;
        }
        else
        {
            pitch = Mathf.Clamp(pitch, pitchClamp.x, pitchClamp.y);
        }

        transform.rotation = Quaternion.Euler(pitch, yaw, 0f);
    }
}
