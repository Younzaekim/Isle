using UnityEngine;

class MapBlockerController : MonoBehaviour
{
    [SerializeField] private Collider forestCollider;
    [SerializeField] private Collider lakeCollider;

    [SerializeField] private Animal rabbit;
    [SerializeField] private Animal frog;

    private void Update()
    {
        // 토끼 상호작용
        if (rabbit.isInteracted)
        {
            DisableBarrier(forestCollider);
        }

        // 개구리 상호작용
        if (frog.isInteracted)
        {
            DisableBarrier(lakeCollider);
        }
    }

    // 전달받은 콜라이더를 비활성화
    private void DisableBarrier(Collider collider)
    {
        if (collider != null)
            collider.enabled = false;
    }
}

