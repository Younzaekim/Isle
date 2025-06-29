using UnityEngine;

public class PlayerAnimationController_Update : MonoBehaviour
{
    // 애니메이터 컴포넌트
    [SerializeField] private Animator animator;

    // 플레이어 상태를 확인할 컨트롤러
    [SerializeField] private PlayerController_Update playerController;

    // 애니메이터 파라미터 해시값
    private static readonly int IsWalking = Animator.StringToHash("IsWalking");
    private static readonly int IsFlying = Animator.StringToHash("IsFlying");
    private static readonly int IsGrounded = Animator.StringToHash("IsGrounded");
    private static readonly int IsJumping = Animator.StringToHash("IsJumping");
    private static readonly int IsIdle = Animator.StringToHash("IsIdle");

    private void Start()
    {
        // animator나 playerController가 할당되지 않았다면 자동으로 가져옴
        if (animator == null)
            animator = GetComponent<Animator>();

        if (playerController == null)
            playerController = GetComponent<PlayerController_Update>();

        // 필수 컴포넌트가 없으면 에러 출력 후 비활성화
        if (animator == null || playerController == null)
        {
            Debug.LogError("필요한 컴포넌트가 할당되지 않았습니다!");
            enabled = false;
            return;
        }
    }

    private void Update()
    {
        // 매 프레임 애니메이션 상태 갱신
        UpdateAnimationState();
    }

    // 플레이어 상태에 따라 적절한 애니메이션 설정
    private void UpdateAnimationState()
    {
        // 이동 입력 값 확인
        float moveX = Input.GetAxisRaw("Horizontal");
        float moveZ = Input.GetAxisRaw("Vertical");

        // 일정 값 이상이면 이동 중으로 간주
        bool isMoving = Mathf.Abs(moveX) > 0.1f || Mathf.Abs(moveZ) > 0.1f;

        if (playerController.isFlying)
        {
            // 날고 있는 상태
            SetAnimationState(isFlying: true);
        }
        else if (playerController.isGrounded)
        {
            // 지면에 있는 상태
            if (isMoving)
            {
                // 걷는 상태
                SetAnimationState(isWalking: true);
            }
            else
            {
                // 정지 상태 (Idle)
                SetAnimationState(isIdle: true);
            }
        }
        else
        {
            // 점프 중 상태
            SetAnimationState(isJumping: true);
        }
    }

    // 애니메이션 파라미터를 설정
    private void SetAnimationState(bool isIdle = false, bool isWalking = false,
                                   bool isJumping = false, bool isFlying = false)
    {
        animator.SetBool(IsIdle, isIdle);
        animator.SetBool(IsWalking, isWalking);
        animator.SetBool(IsJumping, isJumping);
        animator.SetBool(IsFlying, isFlying);

        // Grounded 여부는 직접 설정
        animator.SetBool(IsGrounded, playerController.isGrounded);
    }
}
