using System.Collections.Generic;
using UnityEngine;

public class RespawnController : MonoBehaviour
{
    // 리스폰 존과 리스폰 위치를 묶어주는 클래스
    [System.Serializable]
    public class ZoneRespawnPoint
    {
        public string zoneTag;         // 존 구분용 태그 (부모 오브젝트에 부여)
        public Transform respawnPoint; // 해당 존의 리스폰 위치 (빈 오브젝트)
    }

    public List<ZoneRespawnPoint> respawnPoints = new List<ZoneRespawnPoint>(); // 리스폰 존 정보 리스트

    private Dictionary<string, Transform> respawnDictionary = new Dictionary<string, Transform>(); // 존 태그와 리스폰 위치를 빠르게 찾기 위한 딕셔너리

    public string currentZone = null;   // 현재 플레이어가 밟고 있는 존 태그
    private Transform lastRespawnPoint; // 마지막으로 성공적으로 밟은 리스폰 포인트
    private float fallThreshold = -8f;  // 낙사 판정 y 좌표 기준

    void Start()
    {
        // 인스펙터에서 설정한 리스트를 딕셔너리로 변환 (빠른 조회용)
        foreach (var point in respawnPoints)
        {
            if (!respawnDictionary.ContainsKey(point.zoneTag))
            {
                respawnDictionary.Add(point.zoneTag, point.respawnPoint);
            }
        }

        // 시작 시 리스트의 첫 번째 리스폰 포인트를 기본값으로 설정
        if (respawnPoints.Count > 0)
        {
            lastRespawnPoint = respawnPoints[0].respawnPoint;
        }
        else
        {
            Debug.LogWarning("리스폰 포인트가 설정 안됨");
            lastRespawnPoint = new GameObject("DefaultRespawnPoint").transform;
            lastRespawnPoint.position = transform.position; // 현재 시작 위치를 기본값으로 설정
        }
    }

    void Update()
    {
        // y 좌표가 낙사 기준보다 낮으면 리스폰
        if (transform.position.y < fallThreshold)
        {
            Respawn();
        }
    }

    void Respawn()
    {
        // currentZone이 유효하면 해당 존 리스폰 포인트로 이동
        if (!string.IsNullOrEmpty(currentZone) && respawnDictionary.ContainsKey(currentZone))
        {
            transform.position = respawnDictionary[currentZone].position;
            lastRespawnPoint = respawnDictionary[currentZone]; // 마지막 리스폰 포인트 갱신
        }
        // currentZone을 찾지 못하면 마지막 리스폰 포인트로 이동
        else
        {
            transform.position = lastRespawnPoint.position;
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        // 충돌된 오브젝트의 부모가 있으면 부모의 태그 확인
        if (collision.gameObject.transform.parent != null)
        {
            string parentTag = collision.gameObject.transform.parent.tag;

            // 부모 태그가 등록된 존이면 currentZone 갱신
            if (respawnDictionary.ContainsKey(parentTag))
            {
                currentZone = parentTag;
            }
        }
    }
}
