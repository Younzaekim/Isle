using UnityEngine;
using UnityEngine.EventSystems;

class UI_ButtonFloat : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private float floatHeight = 10f; // 마우스 오버 시 버튼이 떠오르는 높이
    [SerializeField] private float floatSpeed = 5f; // 떠오르는 속도

    private Vector3 originalPosition; // 버튼 원래 위치 저장 변수
    private bool isHovering = false; // 마우스가 버튼 위에 있는지 여부

    void Start()
    {
        // 시작할 때 현재 로컬 위치를 원래 위치로 저장
        originalPosition = transform.localPosition;
    }

    void Update()
    {
        // 마우스가 버튼 위에 있으면 원래 위치에서 floatHeight 만큼 위로 이동 목표 설정
        // 아니면 원래 위치로 이동 목표 설정
        Vector3 target = isHovering ? originalPosition + Vector3.up * floatHeight : originalPosition;

        // 현재 위치에서 목표 위치로 부드럽게 선형 보간하여 이동 (unscaledDeltaTime 사용해 일시정지 영향을 안 받음)
        transform.localPosition = Vector3.Lerp(transform.localPosition, target, Time.unscaledDeltaTime * floatSpeed);
    }

    // 마우스 포인터가 버튼 위로 들어왔을 때 호출되는 이벤트 함수
    public void OnPointerEnter(PointerEventData eventData)
    {
        isHovering = true; // 플로팅 효과 활성화
    }

    // 마우스 포인터가 버튼에서 나갔을 때 호출되는 이벤트 함수
    public void OnPointerExit(PointerEventData eventData)
    {
        isHovering = false; // 플로팅 효과 비활성화
    }
}
