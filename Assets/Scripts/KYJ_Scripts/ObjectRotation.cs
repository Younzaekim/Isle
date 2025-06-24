using UnityEngine;

public class ObjectRotation : MonoBehaviour
{
    [Header("회전 활성화 설정")]
    [SerializeField] private bool rotateX = false;  // X축 회전 활성화
    [SerializeField] private bool rotateY = false;  // Y축 회전 활성화
    [SerializeField] private bool rotateZ = false;  // Z축 회전 활성화

    [Header("회전 속도 설정")]
    [SerializeField] private float rotationSpeedX = 0f;  // X축 회전 속도
    [SerializeField] private float rotationSpeedY = 0f;  // Y축 회전 속도
    [SerializeField] private float rotationSpeedZ = 0f;  // Z축 회전 속도

    // Update is called once per frame
    void Update()
    {
        // 각 축별 회전 값 계산
        float rotationX = rotateX ? rotationSpeedX * Time.deltaTime : 0f;
        float rotationY = rotateY ? rotationSpeedY * Time.deltaTime : 0f;
        float rotationZ = rotateZ ? rotationSpeedZ * Time.deltaTime : 0f;

        // 오브젝트 회전 적용
        transform.Rotate(new Vector3(rotationX, rotationY, rotationZ));
    }
}
