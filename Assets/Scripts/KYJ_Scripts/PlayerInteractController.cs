using UnityEngine;

public class PlayerInteractController : MonoBehaviour
{
    [Header("프리팹 설정")]
    [SerializeField] private GameObject prefabToSpawn; // 생성할 프리팹
    // [SerializeField] private float destroyDelay = 3f;  // 제거될 시간 (초)
    
    [Header("생성 위치 설정")]
    [SerializeField] private Vector3 spawnOffset = new Vector3(0, 0, 0); // 플레이어 기준 생성 위치 오프셋
    
    private GameObject currentSpawnedObject; // 현재 생성된 오브젝트 추적

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.LeftShift) || Input.GetKeyDown(KeyCode.RightShift))
        {
            SpawnTemporaryObject();
        }
    }

    private void SpawnTemporaryObject()
    {
        // 이미 생성된 오브젝트가 있다면 제거
        if (currentSpawnedObject != null)
        {
            Destroy(currentSpawnedObject);
        }

        // 플레이어 위치 + 오프셋에 프리팹 생성
        Vector3 spawnPosition = transform.position + transform.TransformDirection(spawnOffset);
        currentSpawnedObject = Instantiate(prefabToSpawn, spawnPosition, transform.rotation, transform);

        // 지정된 시간 후 제거
        // Destroy(currentSpawnedObject, destroyDelay);
    }
}
