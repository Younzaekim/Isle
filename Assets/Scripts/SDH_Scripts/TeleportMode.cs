using UnityEngine;

public class TeleportMode : MonoBehaviour
{
    // 텔레포트 대상 위치들 (빈 오브젝트로 설정)
    [SerializeField] private Transform[] teleportTargets;

    // 텔레포트에 사용할 키 배열 (예: 숫자 키 1, 2, 3 등)
    [SerializeField] private KeyCode[] teleportKeys;

    private void Update()
    {
        HandleTeleport();
    }

    private void HandleTeleport()
    {
        // 각 텔레포트 위치에 대응하는 키 입력 확인
        for (int i = 0; i < teleportTargets.Length && i < teleportKeys.Length; i++)
        {
            // 지정된 키가 눌렸을 경우 해당 위치로 텔레포트
            if (Input.GetKeyDown(teleportKeys[i]))
            {
                transform.position = teleportTargets[i].position;
            }
        }
    }
}
