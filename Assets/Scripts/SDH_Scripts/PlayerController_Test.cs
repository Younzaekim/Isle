using Unity.Cinemachine;
using UnityEngine;

public class PlayerController_Test : MonoBehaviour
{
    [Header("이동 및 점프")]
    [SerializeField] private float moveSpeed = 8f;
    [SerializeField] private float jumpForce = 6f;
    [SerializeField] private float mouseSensitivity = 3f;
    [SerializeField] private float fallSpeed = 1.5f;

    [Header("비행 관련")]
    [SerializeField] public float flyingTime = 1.5f; // 기본 비행 지속 시간
    [SerializeField] private float flyForce = 50f;   // 초기 비행 힘
    [SerializeField] private float minFlyForce = 5f; // 비행 후반의 최소 힘

    [Header("카메라")]
    [SerializeField] private Transform camTarget;

    [Header("바닥 체크 관련")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundDistance = 0.5f;
    [SerializeField] private LayerMask groundMask;

    private bool canFly = true;
    private bool isSuperFlyMode = false; // F 키로 무제한 비행 모드 토글
    private Rigidbody rb;

    public bool isGrounded { get; private set; } = true;  // 땅에 붙어 있는지 여부
    public bool isFlying { get; private set; } = false;   // 비행 중인지 여부
    public bool isAirborne { get; private set; } = false; // 공중에 있는 상태 (점프 또는 비행)
    public float flyingTimer { get; private set; } = 0f;  // 비행 시간 누적

    private void Start()
    {
        // Rigidbody 컴포넌트 캐싱
        rb = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        // 바닥 체크
        GroundCheck();

        // 점프 및 비행 시작
        Jump();

        // 비행 지속 처리
        FlyHandler();

        // Super Fly 모드 토글
        SuperFlyToggle();
    }

    private void FixedUpdate()
    {
        // 실제 이동 처리 (물리 기반)
        Move();
    }

    private void SuperFlyToggle()
    {
        // F 키로 Super Fly 모드 토글
        if (Input.GetKeyDown(KeyCode.F))
        {
            isSuperFlyMode = !isSuperFlyMode;

            if (isSuperFlyMode)
            {
                flyingTime = Mathf.Infinity; // 비행 시간 무제한
                flyForce = 500;              // 비행 시작 힘 강화
            }
            else
            {
                flyingTime = 1.5f;           // 비행 시간 기본값 복구
                flyForce = 50;               // 비행 시작 힘 복구
            }
        }
    }

    private void Move()
    {
        // 입력 받기
        float moveXInput = Input.GetAxisRaw("Horizontal");
        float moveZInput = Input.GetAxisRaw("Vertical");

        // 카메라 방향 기준 이동 벡터 계산
        Vector3 camZDir = camTarget.forward;
        Vector3 camXDir = camTarget.right;
        camZDir.y = 0f;
        camXDir.y = 0f;
        camZDir.Normalize();
        camXDir.Normalize();

        Vector3 moveDir = camZDir * moveZInput + camXDir * moveXInput;
        Vector3 velocity = moveDir.normalized * moveSpeed;
        velocity.y = rb.linearVelocity.y;

        // 최종 속도 적용
        rb.linearVelocity = velocity;

        // 공중에서 이동 중 회전 및 기울기 처리
        if (isAirborne)
        {
            if (moveDir != Vector3.zero)
            {
                // y축: 카메라 방향으로 서서히 회전
                Quaternion targetYRotation = Quaternion.LookRotation(moveDir, Vector3.up);
                float yRotation = Mathf.LerpAngle(transform.eulerAngles.y, targetYRotation.eulerAngles.y, Time.deltaTime * 1.5f);

                // z축: 좌우 이동 입력에 따라 기울기 적용
                float targetZRotation = -moveXInput * 20f;
                float zRotation = Mathf.LerpAngle(transform.eulerAngles.z, targetZRotation, Time.deltaTime * 8f);

                transform.rotation = Quaternion.Euler(0f, yRotation, zRotation);
            }
            else
            {
                // 이동 입력이 없으면 z축 기울기 복원
                float zRotation = Mathf.LerpAngle(transform.eulerAngles.z, 0f, Time.deltaTime * 5f);
                transform.rotation = Quaternion.Euler(0f, transform.eulerAngles.y, zRotation);
            }
        }
        else
        {
            // 지상에서는 빠르게 회전
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

        // y속도 제거 후 중력 제거
        rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
        rb.useGravity = false;
    }

    private void FlyHandler()
    {
        if (isFlying)
        {
            flyingTimer += Time.deltaTime;

            // 비행 시간이 다 되었으면 비행 종료
            if (flyingTimer >= flyingTime)
            {
                StopFly();
                return;
            }

            // 비행 힘 선형 감소
            float t = flyingTimer / flyingTime;
            float currentForce = Mathf.Lerp(flyForce, minFlyForce, t);

            // 전방 방향으로 힘 가함
            Vector3 flyDirection = transform.forward;
            rb.AddForce(flyDirection * currentForce, ForceMode.Force);

            // Space 누르면 상승, 아니면 하강
            if (Input.GetKey(KeyCode.Space))
            {
                rb.linearVelocity = new Vector3(rb.linearVelocity.x, moveSpeed, rb.linearVelocity.z);
            }
            else
            {
                rb.linearVelocity = new Vector3(rb.linearVelocity.x, -fallSpeed, rb.linearVelocity.z);
            }
        }
    }

    private void StopFly()
    {
        // 비행 종료
        isFlying = false;
        rb.useGravity = true;
    }

    private void GroundCheck()
    {
        // 구체 기반 바닥 감지
        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);

        if (isGrounded)
        {
            // 바닥에 닿으면 비행 가능 및 상태 초기화
            isGrounded = true;
            canFly = true;
            isAirborne = false;
            flyingTimer = 0f;
            StopFly();
        }
        else
        {
            isGrounded = false;
        }
    }

    private void OnDrawGizmos()
    {
        // 에디터에서 GroundCheck 확인용 기즈모
        if (groundCheck != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(groundCheck.position, groundDistance);
        }
    }
}
