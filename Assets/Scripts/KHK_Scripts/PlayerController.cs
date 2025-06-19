using Unity.Cinemachine;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float jumpForce = 5f;
    [SerializeField] private float mouseSensitivity = 2f;
    [SerializeField] private float fallSpeed = 0.5f;
    [SerializeField] private float flyingTime = 7f;
    [SerializeField] private Transform camTarget;

    private Rigidbody rb;
    private bool isGrounded = true;
    private bool canFly = true;
    private bool isFlying = false;

    private float flyingTimer = 0f;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void Update()
    {
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
                //날고있을때 왼쪽 쉬프트 누르면 상승
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
}