using Unity.Cinemachine;
using UnityEngine;

public class PlayerController_Test : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 8f;
    [SerializeField] private float jumpForce = 6f;
    [SerializeField] private float mouseSensitivity = 3f;
    [SerializeField] private float fallSpeed = 1.5f;
    [SerializeField] private float flyingTime = 1.5f;
    [SerializeField] private Transform camTarget;

    [SerializeField] private Transform groundCheck; // 바닥 체크 지점
    [SerializeField] private float groundDistance = 0.5f; // 구체 반지름
    [SerializeField] private LayerMask groundMask; // Ground 레이어

    private Rigidbody rb;
    public bool isGrounded { get; private set; } = true;
    private bool canFly = true;
    public bool isFlying { get; private set; } = false;
    public bool isAirborne { get; private set; } = false;
    private float flyingTimer = 0f;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        GroundCheck();
        Jump();
        FlyHandler();
    }

    private void FixedUpdate()
    {
        Move();
    }

    private void Move()
    {
        float moveXInput = Input.GetAxisRaw("Horizontal");
        float moveZInput = Input.GetAxisRaw("Vertical");

        Vector3 camZDir = camTarget.forward;
        Vector3 camXDir = camTarget.right;

        camZDir.y = 0f;
        camXDir.y = 0f;

        camZDir.Normalize();
        camXDir.Normalize();

        Vector3 moveDir = camZDir * moveZInput + camXDir * moveXInput;
        Vector3 velocity = moveDir.normalized * moveSpeed;
        velocity.y = rb.linearVelocity.y;

        rb.linearVelocity = velocity;

        if (isAirborne)
        {
            if (moveDir != Vector3.zero)
            {
                // 비행 중 이동 입력이 있을 때:
                // y축: 카메라 기준 방향을 천천히 따라감
                Quaternion targetYRotation = Quaternion.LookRotation(moveDir, Vector3.up);
                float yRotation = Mathf.LerpAngle(transform.eulerAngles.y, targetYRotation.eulerAngles.y, Time.deltaTime * 1.5f); // 부드럽게 회전

                // z축: 좌우 입력에 따라 빠르게 기울임
                float targetZRotation = -moveXInput * 20f;
                float zRotation = Mathf.LerpAngle(transform.eulerAngles.z, targetZRotation, Time.deltaTime * 8f); // 빠르게 기울이기

                // 최종 회전 적용
                Quaternion finalRotation = Quaternion.Euler(0f, yRotation, zRotation);
                transform.rotation = finalRotation;
            }
            else
            {
                // 비행 중 이동 입력이 없을 때:
                // z축 기울기만 서서히 복구
                float zRotation = Mathf.LerpAngle(transform.eulerAngles.z, 0f, Time.deltaTime * 5f);
                Quaternion finalRotation = Quaternion.Euler(0f, transform.eulerAngles.y, zRotation);
                transform.rotation = finalRotation;
            }
        }
        else
        {
            // 지상 이동 중:
            // y축 방향을 빠르게 카메라 기준으로 따라감
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
            //땅에 붙어있을땐 점프
            if (isGrounded)
            {
                rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
                isGrounded = false;
                canFly = true;
            }
            //붙어있지 않고 날 수 있으면 날기 시작
            else if (canFly)
            {
                StartFly();
            }
        }
    }

    [SerializeField] private float flyForce = 50f; // 시작 힘
    [SerializeField] private float minFlyForce = 5f; // 최소 힘 (유지되는 힘)

    private void StartFly()
    {
        isFlying = true;
        canFly = false;
        isAirborne = true;
        flyingTimer = 0f;

        rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
        rb.useGravity = false;
    }

    private void FlyHandler()
    {
        if (isFlying)
        {
            flyingTimer += Time.deltaTime;

            if (flyingTimer >= flyingTime)
            {
                StopFly();
                return;
            }

            // 시간에 따라 힘 감소 (선형 감속)
            float t = flyingTimer / flyingTime; // 진행 비율 (0 ~ 1)
            float currentForce = Mathf.Lerp(flyForce, minFlyForce, t); // 힘이 점점 줄어듦

            Vector3 flyDirection = transform.forward;
            rb.AddForce(flyDirection * currentForce, ForceMode.Force); // 지속 감속 추진

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
        isFlying = false;
        rb.useGravity = true;
    }

    private void GroundCheck()
    {
        // 구체 충돌 체크
        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);

        if (isGrounded)
        {
            // 땅에 닿아있는 경우
            isGrounded = true;
            canFly = true;
            isAirborne = false;
            StopFly();
        }
        else
        {
            // 공중에 떠 있는 경우
            isGrounded = false;
        }
    }

    private void OnDrawGizmos()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(groundCheck.position, groundDistance);
        }
    }

    /*
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            //땅에 닿으면 날기 멈춤
            isGrounded = true;
            canFly = true;
            StopFly();
        }
    }
    
    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            //땅에서 떨어지면 날 수 있음
            isGrounded = false;
        }
    }
    */
}
