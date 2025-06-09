using UnityEngine;
using UnityEngine.InputSystem; // Input System 네임스페이스 추가

public class Temp_PlayerController : MonoBehaviour
{
    [Header("이동 설정")]
    [SerializeField] private float moveSpeed = 5.0f; // 플레이어 이동 속도
    [SerializeField] private float rotationSpeed = 100.0f; // 플레이어 회전 속도 (카메라 기준)
    [SerializeField] private float gravity = -9.81f; // 중력
    [SerializeField] private CharacterController characterController; // CharacterController 컴포넌트

    [Header("카메라 설정")]
    [SerializeField] private Transform cameraTransform; // 메인 카메라의 Transform
    [SerializeField] private float mouseSensitivity = 2.0f; // 마우스 감도
    [SerializeField] private float upperLimit = 80f; // 위쪽 회전 제한 (도)
    [SerializeField] private float lowerLimit = -80f; // 아래쪽 회전 제한 (도)

    // Input System 관련 변수
    private PlayerControls playerControls; // 생성한 Input Actions 에셋의 C# 클래스 인스턴스
    private Vector2 movementInput; // WASD 이동 입력 값
    private Vector2 lookInput; // 마우스 회전 입력 값

    private Vector3 velocity; // 플레이어의 현재 속도 (중력 적용용)
    private float cameraPitch = 0f; // 카메라 상하 회전각

    void Awake()
    {
        // CharacterController가 할당되었는지 확인
        if (characterController == null)
        {
            characterController = GetComponent<CharacterController>();
            if (characterController == null)
            {
                Debug.LogError("CharacterController가 할당되지 않았습니다. 스크립트가 부착된 오브젝트에 CharacterController를 추가해주세요.");
                enabled = false; // 스크립트 비활성화
                return;
            }
        }

        // 카메라 Transform 할당 확인 (할당되어 있지 않으면 메인 카메라 찾기)
        if (cameraTransform == null)
        {
            Camera mainCam = Camera.main;
            if (mainCam != null)
            {
                cameraTransform = mainCam.transform;
            }
            else
            {
                Debug.LogError("Camera Transform이 할당되지 않았고, 'MainCamera' 태그를 가진 카메라를 찾을 수 없습니다. 카메라를 할당해주세요.");
                enabled = false; // 스크립트 비활성화
                return;
            }
        }

        // Input Actions 인스턴스 생성 및 액션 바인딩
        playerControls = new PlayerControls();

        // Movement 액션 구독
        playerControls.Player.Move.performed += ctx => movementInput = ctx.ReadValue<Vector2>();
        playerControls.Player.Move.canceled += ctx => movementInput = Vector2.zero; // 키 떼면 입력 0으로

        // Look 액션 구독 추가
        playerControls.Player.Look.performed += ctx => lookInput = ctx.ReadValue<Vector2>();
        playerControls.Player.Look.canceled += ctx => lookInput = Vector2.zero;

        // 커서 잠금
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void OnEnable()
    {
        // Input Actions 활성화
        playerControls.Enable();
    }

    void OnDisable()
    {
        // Input Actions 비활성화
        playerControls.Disable();
    }

    void Update()
    {
        HandleRotation();
        HandleMovement();
        ApplyGravity();
    }

    private void HandleRotation()
    {
        // 마우스 입력으로 카메라 회전
        if (lookInput.sqrMagnitude >= 0.01f)
        {
            // 좌우 회전 (플레이어 전체 회전)
            float yaw = lookInput.x * mouseSensitivity;
            transform.Rotate(Vector3.up, yaw);

            // 상하 회전 (카메라만 회전)
            cameraPitch -= lookInput.y * mouseSensitivity;
            cameraPitch = Mathf.Clamp(cameraPitch, lowerLimit, upperLimit);
            cameraTransform.localRotation = Quaternion.Euler(cameraPitch, 0, 0);
        }
    }

    private void HandleMovement()
    {
        Vector3 moveDirection = Vector3.zero;
        if (movementInput.sqrMagnitude > 0.01f)
        {
            // 플레이어의 정면과 오른쪽 방향을 기준으로 이동
            Vector3 forward = transform.forward;
            Vector3 right = transform.right;

            moveDirection = (forward * movementInput.y + right * movementInput.x).normalized;
        }

        // 이동 적용
        characterController.Move(moveDirection * moveSpeed * Time.deltaTime);
    }

    private void ApplyGravity()
    {
        if (characterController.isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }
        velocity.y += gravity * Time.deltaTime;
        characterController.Move(velocity * Time.deltaTime);
    }
}