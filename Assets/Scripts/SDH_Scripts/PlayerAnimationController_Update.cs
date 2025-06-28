using UnityEngine;

public class PlayerAnimationController_Update : MonoBehaviour
{
    [SerializeField] private Animator animator;
    [SerializeField] private PlayerController_Update playerController;

    // 애니메이터 파라미터 해시값
    private static readonly int IsWalking = Animator.StringToHash("IsWalking");
    private static readonly int IsFlying = Animator.StringToHash("IsFlying");
    private static readonly int IsGrounded = Animator.StringToHash("IsGrounded");
    private static readonly int IsJumping = Animator.StringToHash("IsJumping");
    private static readonly int IsIdle = Animator.StringToHash("IsIdle");

    private void Start()
    {
        // 컴포넌트가 없다면 자동으로 가져오기
        if (animator == null)
            animator = GetComponent<Animator>();

        if (playerController == null)
            playerController = GetComponent<PlayerController_Update>();

        if (animator == null || playerController == null)
        {
            Debug.LogError("필요한 컴포넌트가 할당되지 않았습니다!");
            enabled = false;
            return;
        }
    }

    private void Update()
    {
        UpdateAnimationState();
    }

    private void UpdateAnimationState()
    {
        // 이동 입력 확인
        float moveX = Input.GetAxisRaw("Horizontal");
        float moveZ = Input.GetAxisRaw("Vertical");
        bool isMoving = Mathf.Abs(moveX) > 0.1f || Mathf.Abs(moveZ) > 0.1f;

        if (playerController.isFlying)
        {
            // 날고 있을 때
            SetAnimationState(isFlying: true);
        }
        else if (playerController.isGrounded)
        {
            // 땅에 있을 때
            if (isMoving)
            {
                // 걷기
                SetAnimationState(isWalking: true);
            }
            else
            {
                // 정지(Idle)
                SetAnimationState(isIdle: true);
            }
        }
        else
        {
            // 점프 중일 때
            SetAnimationState(isJumping: true);
        }
    }

    private void SetAnimationState(bool isIdle = false, bool isWalking = false, 
                                 bool isJumping = false, bool isFlying = false)
    {
        animator.SetBool(IsIdle, isIdle);
        animator.SetBool(IsWalking, isWalking);
        animator.SetBool(IsJumping, isJumping);
        animator.SetBool(IsFlying, isFlying);
        animator.SetBool(IsGrounded, playerController.isGrounded);
    }
}
