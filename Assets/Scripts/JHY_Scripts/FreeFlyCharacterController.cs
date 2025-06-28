using UnityEngine;

public class FreeFlyCharacterController : MonoBehaviour
{
    [Header("기본 이동 설정")]
    [SerializeField] private float moveSpeed = 10f; // 기본 이동 속도
    [SerializeField] private float rotationSpeed = 2f; // 회전 속도 (마우스 시점 제어)

    [Header("상승/하강 설정")]
    [SerializeField] private float ascendSpeed = 15f; // 상승 속도
    [SerializeField] private float descendSpeed = 15f; // 하강 속도

    [Header("가속 설정 (Shift)")]
    [SerializeField] private float accelerationFactor = 2.5f; // 가속 배율
    [SerializeField] private float maxSpeed = 50f; // 최대 속도 (가속 시)

    private Rigidbody rb;
    private float currentMoveSpeed; // 현재 이동 속도

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            Debug.LogError("Rigidbody 컴포넌트가 필요합니다. FreeFlyCharacterController 스크립트가 있는 게임 오브젝트에 Rigidbody를 추가해주세요.");
            enabled = false; // Rigidbody가 없으면 스크립트 비활성화
            return;
        }

        // Rigidbody 설정
        rb.useGravity = false; // 자유롭게 날기 위해 중력 비활성화
        rb.linearDamping = 5f; // 공기 저항을 추가하여 가속 후 떼면 서서히 멈추도록

        currentMoveSpeed = moveSpeed;

        // 마우스 커서 숨기기 및 잠금 (게임 실행 시)
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        HandleRotationInput(); // Update에서 즉각적인 회전 처리
        HandleAcceleration(); // Update에서 즉각적인 속도 조절

        // Esc 키를 누르면 커서 잠금 해제 및 보이기 (테스트 중 종료 편의를 위해)
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    void FixedUpdate()
    {
        // 물리 업데이트는 FixedUpdate에서 처리
        ApplyMovement();
        ApplyVerticalMovement();
        LimitSpeed();
    }

    private void HandleRotationInput()
    {
        // 마우스로 시점 회전 (Yaw - Y축, Pitch - X축)
        float mouseX = Input.GetAxis("Mouse X") * rotationSpeed;
        float mouseY = Input.GetAxis("Mouse Y") * rotationSpeed;

        // Yaw (Y축 회전)
        transform.Rotate(Vector3.up, mouseX, Space.World);

        // Pitch (X축 회전) - 카메라가 바라보는 방향에 따라 위/아래를 향하도록
        transform.Rotate(Vector3.left, mouseY, Space.Self);
    }

    private void HandleAcceleration()
    {
        // Shift 키를 누르면 가속, 떼면 원상 복귀
        if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
        {
            currentMoveSpeed = Mathf.Lerp(currentMoveSpeed, maxSpeed, Time.deltaTime * accelerationFactor);
        }
        else
        {
            currentMoveSpeed = Mathf.Lerp(currentMoveSpeed, moveSpeed, Time.deltaTime * accelerationFactor);
        }
    }

    private void ApplyMovement()
    {
        // WASD 키로 전후좌우 이동 입력
        float horizontalInput = Input.GetAxis("Horizontal"); // A/D
        float verticalInput = Input.GetAxis("Vertical");     // W/S

        // 현재 바라보는 방향을 기준으로 전후좌우 벡터 계산
        Vector3 desiredDirection = transform.right * horizontalInput + transform.forward * verticalInput;

        if (desiredDirection.magnitude > 0.1f) // 입력이 있을 때만 힘 적용
        {
            // 원하는 방향으로 힘을 가하여 속도 제어
            // ForceMode.Acceleration은 질량에 무관하게 가속도를 적용합니다.
            rb.AddForce(desiredDirection.normalized * currentMoveSpeed, ForceMode.Acceleration);
        }
        else
        {
            // 입력이 없으면 수평 방향 속도를 서서히 줄임 (drag가 이 역할을 하기도 함)
            // 필요하다면 추가적인 감속 로직을 여기에 넣을 수 있습니다.
        }
    }

    private void ApplyVerticalMovement()
    {
        // 컨트롤 키 (Ctrl)로 상승, 스페이스바 (Space)로 하강
        if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))
        {
            // 선형 속도의 Y축만 직접 설정하여 상승
            // AddForce를 사용할 수도 있지만, 직접적인 상승/하강 속도 제어를 위해 linearVelocity.y를 설정
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, ascendSpeed, rb.linearVelocity.z);
        }
        else if (Input.GetKey(KeyCode.Space)) // Space bar for descending
        {
            // 선형 속도의 Y축만 직접 설정하여 하강
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, -descendSpeed, rb.linearVelocity.z);
        }
        else
        {
            // 수직 입력이 없을 때 Y축 속도를 서서히 0으로 (드래그 효과와 유사)
            // Rigidbody.drag가 이미 수평/수직 모두에 적용되므로,
            // 굳이 이 부분에서 추가적인 감속을 하지 않아도 됩니다.
            // 하지만 즉각적인 Y축 속도 초기화를 원한다면:
            // rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);
        }
    }

    private void LimitSpeed()
    {
        // 최대 속도 제한
        // 현재 선형 속도의 XZ 평면 성분만 가져와서 Magnitude를 계산
        Vector3 horizontalVelocity = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);
        if (horizontalVelocity.magnitude > maxSpeed)
        {
            horizontalVelocity = horizontalVelocity.normalized * maxSpeed;
            // 제한된 수평 속도와 기존의 수직 속도를 조합하여 linearVelocity 설정
            rb.linearVelocity = new Vector3(horizontalVelocity.x, rb.linearVelocity.y, horizontalVelocity.z);
        }

        // 선택 사항: 수직 속도도 제한하고 싶다면 (예: maxAscendSpeed, maxDescendSpeed 추가)
        // if (Mathf.Abs(rb.linearVelocity.y) > someMaxVerticalSpeed) { ... }
    }
}