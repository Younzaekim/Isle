using UnityEngine;

public class CamController : MonoBehaviour
{
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
        pitch = Mathf.Clamp(pitch, pitchClamp.x, pitchClamp.y);

        transform.rotation = Quaternion.Euler(pitch, yaw, 0f);
    }
}
