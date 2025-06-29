using Unity.Cinemachine;
using UnityEngine;

public class PlayerController_Update : MonoBehaviour
{
    [Header("Movement and Jump")]
    [SerializeField] private float moveSpeed = 8f;        // 이동 속도
    [SerializeField] private float upwardLift = 10f;      // 비행 시 상승 힘
    [SerializeField] private float jumpForce = 8f;        // 점프 힘
    [SerializeField] private float mouseSensitivity = 3f; // 마우스 감도 (사용하지 않음)
    [SerializeField] private float fallSpeed = 3f;        // 낙하 속도

    [Header("동물 상호작용 설정")]
    [SerializeField] private float interactDistance = 2f;
    [SerializeField] private LayerMask whatIsAnimal;

    [Header("Flying")]
    [SerializeField] public float flyingTime = 3f;     // 기본 비행 가능 시간
    [SerializeField] private float forwardLift = 180f; // 비행 시 전방 추진력
    [SerializeField] private float minFlyForce = 50f;  // 비행 후반 최소 추진력

    [Header("Camera")]
    [SerializeField] private Transform camTarget; // 카메라 기준 회전용 타겟

    [Header("GroundCheck")]
    [SerializeField] private Transform groundCheck;       // 바닥 체크용 위치
    [SerializeField] private float groundDistance = 0.4f; // 바닥 체크 거리
    [SerializeField] private LayerMask groundMask;        // 어떤 레이어가 바닥인지 지정

    private bool canFly = true;          // 비행 가능 여부
    private bool isSuperFlyMode = false; // 무제한 비행 모드
    private Rigidbody rb;                // 리지드바디 컴포넌트

    // 상태 프로퍼티
    public bool isGrounded { get; private set; } = true;
    public bool isFlying { get; private set; } = false;
    public bool isAirborne { get; private set; } = false;
    public float flyingTimer { get; private set; } = 0f;

    private void Start()
    {
        // Rigidbody 캐싱
        rb = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        GroundCheck();    // 땅 체크
        Jump();           // 점프 및 비행 시작
        FlyHandler();     // 비행 지속 처리
        SuperFlyToggle(); // 무제한 비행 모드 토글
    }

    private void FixedUpdate()
    {
        Move(); // 물리 기반 이동
    }

    private void SuperFlyToggle()
    {
        // F 키로 무제한 비행 모드 토글
        if (Input.GetKeyDown(KeyCode.F))
        {
            isSuperFlyMode = !isSuperFlyMode;

            if (isSuperFlyMode)
            {
                flyingTime = Mathf.Infinity;
                forwardLift = 500f;
            }
            else
            {
                flyingTime = 3f;
                forwardLift = 180f;
            }
        }
    }

    private void Move()
    {
        // 입력 받기
        float moveXInput = Input.GetAxisRaw("Horizontal");
        float moveZInput = Input.GetAxisRaw("Vertical");

        // 카메라 방향 기준 이동 방향 계산
        Vector3 camZDir = camTarget.forward;
        Vector3 camXDir = camTarget.right;
        camZDir.y = 0f;
        camXDir.y = 0f;
        camZDir.Normalize();
        camXDir.Normalize();

        // 이동 방향 및 속도 계산
        Vector3 moveDir = camZDir * moveZInput + camXDir * moveXInput;
        Vector3 velocity = moveDir.normalized * moveSpeed;
        velocity.y = rb.linearVelocity.y;

        // 속도 적용
        rb.linearVelocity = velocity;

        // 공중 회전 처리
        if (isAirborne)
        {
            if (moveDir != Vector3.zero)
            {
                // y 회전 (카메라 방향으로)
                Quaternion targetYRotation = Quaternion.LookRotation(moveDir, Vector3.up);
                float yRotation = Mathf.LerpAngle(transform.eulerAngles.y, targetYRotation.eulerAngles.y, Time.deltaTime * 1.5f);

                // x 회전 (앞으로 기울임)
                float targetXRotation = isFlying ? Mathf.Lerp(0f, -30f, flyingTimer / flyingTime) : 0f;
                float xRotation = Mathf.LerpAngle(transform.eulerAngles.x, targetXRotation, Time.deltaTime * 5f);

                // z 회전 (좌우 기울기)
                float targetZRotation = -moveXInput * 20f;
                float zRotation = Mathf.LerpAngle(transform.eulerAngles.z, targetZRotation, Time.deltaTime * 8f);

                transform.rotation = Quaternion.Euler(xRotation, yRotation, zRotation);
            }
            else
            {
                // 정면 복구 회전
                Quaternion currentRot = transform.rotation;
                Quaternion targetRot = Quaternion.Euler(0f, transform.eulerAngles.y, 0f);
                float maxDegreesDelta = upwardLift * Time.deltaTime;
                transform.rotation = Quaternion.RotateTowards(currentRot, targetRot, maxDegreesDelta);
            }
        }
        else
        {
            // 지상에서는 빠른 회전
            if (moveDir != Vector3.zero)
            {
                Quaternion toRotation = Quaternion.LookRotation(moveDir, Vector3.up);
                transform.rotation = Quaternion.Slerp(transform.rotation, toRotation, Time.deltaTime * 10f);
            }
        }
    }

    private void Jump()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (isGrounded)
            {
                // 점프
                rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
                isGrounded = false;
                canFly = true;
            }
            else if (canFly)
            {
                // 비행 시작
                StartFly();
            }
        }
    }

    private void StartFly()
    {
        isFlying = true;
        canFly = false;
        isAirborne = true;
        flyingTimer = 0f;

        // 수직 속도 초기화 및 중력 제거
        rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
        rb.useGravity = false;
    }

    private void FlyHandler()
    {
        if (isAirborne)
        {
            float t = Mathf.Clamp01(flyingTimer / flyingTime);
            float currentForwardForce = Mathf.Lerp(forwardLift, minFlyForce, t);
            Vector3 targetVelocity = transform.forward * currentForwardForce;

            bool canFlyUp = flyingTimer < flyingTime;

            if (Input.GetKey(KeyCode.LeftControl))
            {
                // 급강하
                isFlying = false;
                targetVelocity.y = -fallSpeed * 3f;
            }
            else if (Input.GetKey(KeyCode.Space) && canFlyUp)
            {
                // 상승 비행
                isFlying = true;
                flyingTimer += Time.deltaTime;
                targetVelocity.y = upwardLift;
            }
            else
            {
                // 낙하
                isFlying = false;
                targetVelocity.y = -fallSpeed;
            }

            rb.linearVelocity = targetVelocity;

            // 비행 시간 종료 시
            if (flyingTimer >= flyingTime)
            {
                StopFly();
            }
        }
    }

    private void StopFly()
    {
        // 비행 종료 처리
        isFlying = false;
        rb.useGravity = true;
    }

    private void GroundCheck()
    {
        // 구체 기반 바닥 감지
        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);

        if (isGrounded)
        {
            // 바닥에 닿았을 때 상태 초기화
            canFly = true;
            isAirborne = false;
            flyingTimer = 0f;
            StopFly();
        }
    }

    private void OnDrawGizmos()
    {
        // 바닥 체크 영역 확인용 기즈모
        if (groundCheck != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(groundCheck.position, groundDistance);
        }
    }

    private void InteractWithAnimal()
    {
        Vector3 sphereCenter = transform.position + transform.forward * interactDistance;
        float sphereRadius = interactDistance;

        RaycastHit[] hits = Physics.SphereCastAll(transform.position, sphereRadius, transform.forward, interactDistance, whatIsAnimal);
        foreach (var hit in hits)
        {
            Animal animal = hit.collider.GetComponent<Animal>();
            if (animal != null)
            {
                animal.Interact();
                break;
            }
        }
    }
}
