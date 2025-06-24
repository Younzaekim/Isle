using UnityEngine;

public class TeleportMode : MonoBehaviour
{
    [SerializeField] private Transform[] teleportTargets; // 텔레포트할 위치 (빈 오브젝트들)
    [SerializeField] private KeyCode[] teleportKeys; // 텔레포트 키 (1번, 2번 등)

    void Update()
    {
        HandleTeleport();
    }

    private void HandleTeleport()
    {
        for (int i = 0; i < teleportTargets.Length && i < teleportKeys.Length; i++)
        {
            if (Input.GetKeyDown(teleportKeys[i]))
            {
                transform.position = teleportTargets[i].position;
            }
        }
    }
}
