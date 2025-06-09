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

    // Input System 관련 변수
    private PlayerControls playerControls; // 생성한 Input Actions 에셋의 C# 클래스 인스턴스
    private Vector2 movementInput; // WASD 이동 입력 값
    private Vector2 lookInput; // 마우스 회전 입력 값

    private Vector3 velocity; // 플레이어의 현재 속도 (중력 적용용)

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

        // Look 액션 구독 (카메라 회전은 별도 스크립트에서 처리될 수 있으나, 여기서는 플레이어 방향만 고려)
        //playerControls.Player.Look.performed += ctx => lookInput = ctx.ReadValue<Vector2>();
        //playerControls.Player.Look.canceled += ctx => lookInput = Vector2.zero;
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
        // 1. 중력 적용
        if (characterController.isGrounded && velocity.y < 0)
        {
            velocity.y = -2f; // 땅에 닿으면 y 속도를 낮게 유지
        }
        velocity.y += gravity * Time.deltaTime;
        characterController.Move(velocity * Time.deltaTime);

        // 2. 카메라 기준 이동 방향 계산
        Vector3 moveDirection = Vector3.zero;
        if (movementInput.sqrMagnitude > 0.01f) // 입력이 있을 때만
        {
            // 카메라의 정면 방향을 기준으로 이동 (Y축 회전만 고려)
            Vector3 forward = cameraTransform.forward;
            forward.y = 0; // Y축은 무시 (플레이어가 위아래로 기울어지지 않게)
            forward.Normalize(); // 정규화 (길이 1로 만듦)

            // 카메라의 오른쪽 방향 계산
            Vector3 right = cameraTransform.right;
            right.y = 0; // Y축은 무시
            right.Normalize();

            // 입력값 (WASD)에 따라 최종 이동 방향 계산
            // movementInput.y: W(앞) -> 1, S(뒤) -> -1
            // movementInput.x: A(왼쪽) -> -1, D(오른쪽) -> 1
            moveDirection = (forward * movementInput.y + right * movementInput.x).normalized;
        }

        // 3. 플레이어 회전 (선택 사항: 카메라 방향을 따라감)
        // 플레이어 캐릭터가 이동 방향을 향하도록 회전
        if (moveDirection.sqrMagnitude > 0.01f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }

        // 4. 플레이어 이동 적용
        characterController.Move(moveDirection * moveSpeed * Time.deltaTime);
    }
}