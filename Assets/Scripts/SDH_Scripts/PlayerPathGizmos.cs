using System.Collections.Generic;
using UnityEngine;

public class PlayerPathGizmos : MonoBehaviour
{
    // 플레이어가 지나간 위치들을 저장할 리스트
    private List<Vector3> positions = new List<Vector3>();

    // 저장할 최대 위치 개수
    private int maxPositions = 100;

    private void Update()
    {
        // 현재 플레이어 위치를 가져옴
        Vector3 currentPos = transform.position;

        // 처음이거나 마지막 위치와 일정 거리 이상 차이 나면 추가
        if (positions.Count == 0 || Vector3.Distance(positions[positions.Count - 1], currentPos) > 0.1f)
        {
            positions.Add(currentPos);

            // 최대 개수를 초과하면 제일 오래된 위치 제거
            if (positions.Count > maxPositions)
                positions.RemoveAt(0);
        }
    }

    private void OnDrawGizmos()
    {
        // 기즈모 색상을 노란색으로 설정
        Gizmos.color = Color.yellow;

        // 위치 간 선을 그려 이동 경로 시각화
        for (int i = 1; i < positions.Count; i++)
        {
            Gizmos.DrawLine(positions[i - 1], positions[i]);
        }
    }
}
