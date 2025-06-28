using Unity.Cinemachine;
using UnityEngine;

public class CamManager : MonoBehaviour
{
    [SerializeField] private CinemachineCamera cinemachineCamera;
    [SerializeField] private Transform camTarget; // 지상용 타겟 (CamTarget)
    [SerializeField] private Transform playerTarget; // 비행용 타겟 (Player)
    [SerializeField] private PlayerController_Update player;

    void Update()
    {
        if (player == null || cinemachineCamera == null) return;

        if (player.isFlying)
        {
            // 비행 중 → 플레이어 따라감
            cinemachineCamera.Follow = playerTarget;
            cinemachineCamera.LookAt = playerTarget;
        }
        else
        {
            // 지상일 때 → CamTarget 따라감
            cinemachineCamera.Follow = camTarget;
            cinemachineCamera.LookAt = camTarget;
        }
    }
}
