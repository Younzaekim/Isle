using UnityEngine;

class QuestBarrierController : MonoBehaviour
{
    [SerializeField] private Collider quest1Collider;
    [SerializeField] private Collider quest2Collider;

    private void Update()
    {
        // 퀘스트 1 클리어 (예시: Q 키)
        if (Input.GetKeyDown(KeyCode.Q))
        {
            DisableBarrier(quest1Collider);
        }

        // 퀘스트 2 클리어 (예시: E 키)
        if (Input.GetKeyDown(KeyCode.E))
        {
            DisableBarrier(quest2Collider);
        }
    }

    // 전달받은 콜라이더를 비활성화
    private void DisableBarrier(Collider collider)
    {
        if (collider != null)
            collider.enabled = false;
    }
}

