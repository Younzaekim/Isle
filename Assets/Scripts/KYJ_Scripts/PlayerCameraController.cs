using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerCameraController : MonoBehaviour
{
    [SerializeField] private float mouseSensitivity = 2f;
    [SerializeField] private float upperLimit = 80f;
    [SerializeField] private float lowerLimit = -80f;
    
    private float rotationX;
    private PlayerControls playerControls;
    private Vector2 lookInput;

    private void Awake()
    {
        playerControls = new PlayerControls();
        
        // Look 액션 구독
        playerControls.Player.Look.performed += ctx => lookInput = ctx.ReadValue<Vector2>();
        playerControls.Player.Look.canceled += ctx => lookInput = Vector2.zero;

        // 커서 잠금
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void OnEnable()
    {
        playerControls.Enable();
    }

    private void OnDisable()
    {
        playerControls.Disable();
    }

    private void LateUpdate()
    {
        if (lookInput.sqrMagnitude >= 0.01f)
        {
            // 상하 회전 (X축)
            rotationX -= lookInput.y * mouseSensitivity;
            rotationX = Mathf.Clamp(rotationX, lowerLimit, upperLimit);

            // 좌우 회전 (Y축)
            float deltaY = lookInput.x * mouseSensitivity;
            transform.parent.Rotate(Vector3.up, deltaY);

            // 카메라 회전 적용
            transform.localRotation = Quaternion.Euler(rotationX, 0, 0);
        }
    }
}