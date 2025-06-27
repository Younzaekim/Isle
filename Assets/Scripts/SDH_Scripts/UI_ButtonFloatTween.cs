using UnityEngine;
using UnityEngine.EventSystems;

class UI_ButtonFloat : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    Vector3 originalPosition;
    public float floatHeight = 10f;
    public float floatSpeed = 5f;
    bool isHovering = false;

    void Start()
    {
        originalPosition = transform.localPosition;
    }

    void Update()
    {
        Vector3 target = isHovering ? originalPosition + Vector3.up * floatHeight : originalPosition;
        transform.localPosition = Vector3.Lerp(transform.localPosition, target, Time.unscaledDeltaTime * floatSpeed);
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
