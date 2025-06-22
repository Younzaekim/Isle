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

        if (moveDir != Vector3.zero)
        {
            Quaternion toRotation = Quaternion.LookRotation(moveDir, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, toRotation, Time.deltaTime * 10f);
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

    private void StartFly()
    {
        isFlying = true;
        canFly = false;
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

            if (Input.GetKey(KeyCode.Space))
            {
                //날고있을때 스페이스바 누르면 상승
                rb.linearVelocity = new Vector3(rb.linearVelocity.x, moveSpeed, rb.linearVelocity.z);
            }
            else
            {
                //아무것도 누르지 않으면 천천히 하강(활공하는 느낌)
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
