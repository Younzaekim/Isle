using UnityEngine;

class MapBlockerController : MonoBehaviour
{
    // 숲 지역의 접근을 막는 콜라이더
    [SerializeField] private Collider forestCollider;

    // 호수 지역의 접근을 막는 콜라이더
    [SerializeField] private Collider lakeCollider;

    // 토끼 오브젝트 (상호작용 여부를 확인)
    [SerializeField] private Animal rabbit;

    // 개구리 오브젝트 (상호작용 여부를 확인)
    [SerializeField] private Animal frog;

    private void Update()
    {
        // 토끼와 상호작용했으면 숲 콜라이더 비활성화
        if (rabbit.isInteracted)
        {
            DisableBarrier(forestCollider);
        }

        // 개구리와 상호작용했으면 호수 콜라이더 비활성화
        if (frog.isInteracted)
        {
            DisableBarrier(lakeCollider);
        }
    }

    // 전달받은 콜라이더를 비활성화시켜 해당 지역 접근을 허용
    private void DisableBarrier(Collider collider)
    {
        if (collider != null)
            collider.enabled = false;
    }
}
