using UnityEngine;
using UnityEngine.EventSystems;

class UI_ButtonScaleEffect : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    Vector3 originalScale;
    public float scaleFactor = 1.1f; // 1.0 = 원래 크기, 1.1 = 10% 커짐
    public float scaleSpeed = 5f;
    bool isHovering = false;

    void Start()
    {
        originalScale = transform.localScale;
    }

    void Update()
    {
        Vector3 target = isHovering ? originalScale * scaleFactor : originalScale;
        transform.localScale = Vector3.Lerp(transform.localScale, target, Time.deltaTime * scaleSpeed);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        isHovering = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isHovering = false;
    }
}
