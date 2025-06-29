using UnityEngine;
using UnityEngine.EventSystems;

class UI_ButtonScaleEffect : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private float scaleFactor = 1.1f; // 버튼이 커질 배율 (1.0 = 원래 크기, 1.1 = 10% 커짐)
    [SerializeField] private float scaleSpeed = 5f; // 크기 변경 속도

    Vector3 originalScale; // 버튼 원래 크기 저장 변수
    bool isHovering = false; // 마우스가 버튼 위에 있는지 여부

    void Start()
    {
        // 시작 시 현재 로컬 스케일을 원래 크기로 저장
        originalScale = transform.localScale;
    }

    void Update()
    {
        // 마우스가 버튼 위에 있으면 크기를 scaleFactor배로 확대, 아니면 원래 크기로 설정
        Vector3 target = isHovering ? originalScale * scaleFactor : originalScale;
        // 현재 크기에서 목표 크기로 부드럽게 보간하여 변경
        transform.localScale = Vector3.Lerp(transform.localScale, target, Time.deltaTime * scaleSpeed);
    }

    // 마우스 포인터가 버튼 위로 들어왔을 때 호출되는 이벤트
    public void OnPointerEnter(PointerEventData eventData)
    {
        isHovering = true; // 크기 확대 시작
    }

    // 마우스 포인터가 버튼에서 나갔을 때 호출되는 이벤트
    public void OnPointerExit(PointerEventData eventData)
    {
        isHovering = false; // 크기 원래대로 축소 시작
    }
}
