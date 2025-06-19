using UnityEngine;
using UnityEngine.InputSystem; // Input System 네임스페이스 추가

public class Temp_PlayerController : MonoBehaviour
{
    [Header("이동 설정")]
    [SerializeField] private float moveSpeed = 5.0f; // 플레이어 이동 속도
    [SerializeField] private float gravity = -9.81f; // 중력
    [SerializeField] private CharacterController characterController; // CharacterController 컴포넌트

    // Input System 관련 변수
    private PlayerControls playerControls; // 생성한 Input Actions 에셋의 C# 클래스 인스턴스
    private Vector2 movementInput; // WASD 이동 입력 값
    private Vector2 lookInput; // 마우스 회전 입력 값

    private Vector3 velocity; // 플레이어의 현재 속도 (중력 적용용)

    [Header("회전 설정")]
    [SerializeField] private float rotationSpeed = 10f; // 회전 속도

    [Header("점프 설정")]
    [SerializeField] private float jumpForce = 5f;
    [SerializeField] private bool canDoubleJump = false; // 이중 점프 가능 여부
    private bool hasDoubleJumped = false;

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

        // Input Actions 인스턴스 생성 및 액션 바인딩
        playerControls = new PlayerControls();

        // Movement 액션 구독
        playerControls.Player.Move.performed += ctx => movementInput = ctx.ReadValue<Vector2>();
        playerControls.Player.Move.canceled += ctx => movementInput = Vector2.zero; // 키 떼면 입력 0으로

        // Look 액션 구독 (카메라 회전은 별도 스크립트에서 처리될 수 있으나, 여기서는 플레이어 방향만 고려)
        //playerControls.Player.Look.performed += ctx => lookInput = ctx.ReadValue<Vector2>();
        //playerControls.Player.Look.canceled += ctx => lookInput = Vector2.zero;

        // Jump 액션 구독 추가
        playerControls.Player.Jump.performed += ctx => OnJump();
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
        ApplyGravity();
        HandleMovement();
    }

    private void OnJump()
    {
        if (characterController.isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpForce * -2f * gravity);
            hasDoubleJumped = false;
        }
        else if (canDoubleJump && !hasDoubleJumped)
        {
            velocity.y = Mathf.Sqrt(jumpForce * -2f * gravity);
            hasDoubleJumped = true;
        }
    }

    private void ApplyGravity()
    {
        if (characterController.isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
            hasDoubleJumped = false; // 땅에 닿으면 이중 점프 초기화
        }
        velocity.y += gravity * Time.deltaTime;
        characterController.Move(velocity * Time.deltaTime);
    }

    private void HandleMovement()
    {
        Vector3 moveDirection = Vector3.zero;
        if (movementInput.sqrMagnitude > 0.01f)
        {
            // 카메라 기준으로 이동 방향 계산
            Vector3 forward = Camera.main.transform.forward;
            Vector3 right = Camera.main.transform.right;
            
            // 수직 방향은 무시
            forward.y = 0;
            right.y = 0;
            forward.Normalize();
            right.Normalize();

            moveDirection = (forward * movementInput.y + right * movementInput.x).normalized;
        }

        // 이동만 적용 (회전은 제거)
        characterController.Move(moveDirection * moveSpeed * Time.deltaTime);
    }
}