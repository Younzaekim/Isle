using UnityEngine;
using System.Collections.Generic;

public class ChainedPulseObjectPool : MonoBehaviour
{
    public static ChainedPulseObjectPool Instance { get; private set; }

    [Tooltip("풀링할 연쇄 펄스 프리팹")]
    [SerializeField] private GameObject chainedPulsePrefab;
    [Tooltip("초기 생성할 오브젝트 개수")]
    [SerializeField] private int initialPoolSize = 10;

    private Queue<ChainedRadarPulseTrigger> availableObjects = new Queue<ChainedRadarPulseTrigger>();
    private bool isInitialized = false;
    private HashSet<GameObject> _activePulsingTargets = new HashSet<GameObject>();  //중복 생성 방지용

    public bool IsInitialized => isInitialized;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        if (chainedPulsePrefab != null && !isInitialized)
        {
            InitializePool();
        }
    }

    public void InitializePool(GameObject prefab)
    {
        if (prefab == null)
        {
            Debug.LogError("InitializePool: 전달된 프리팹이 null.PlayerRaderPulse에서 할당 필요.", this);
            return;
        }

        this.chainedPulsePrefab = prefab;

        while (availableObjects.Count > 0)
        {
            ChainedRadarPulseTrigger trigger = availableObjects.Dequeue();
            if (trigger != null)
            {
                Destroy(trigger.gameObject);
            }
        }
        availableObjects.Clear();

        for (int i = 0; i < initialPoolSize; i++)
        {
            ChainedRadarPulseTrigger newObject = CreateNewPooledObject();
            if (newObject != null)
            {
                newObject.gameObject.SetActive(false); // 초기에는 비활성화 
                availableObjects.Enqueue(newObject);
            }
        }
        isInitialized = true;
        //Debug.Log($"ChainedPulseObjectPool이 {initialPoolSize}개의 오브젝트로 초기화되었습니다.");
    }

    public void InitializePool()
    {
        InitializePool(chainedPulsePrefab);
    }

    ChainedRadarPulseTrigger CreateNewPooledObject()
    {
        if (chainedPulsePrefab == null)
        {
            Debug.LogError("풀링할 프리팹이 설정되지 않음", this);
            return null;
        }

        GameObject obj = Instantiate(chainedPulsePrefab, transform);
        ChainedRadarPulseTrigger trigger = obj.GetComponent<ChainedRadarPulseTrigger>();
        if (trigger == null)
        {
            Debug.LogError($"프리팹 '{chainedPulsePrefab.name}'에 ChainedRadarPulseTrigger 컴포넌트가 없음! 잘못된 프리팹 할당!", chainedPulsePrefab);
            Destroy(obj);
            return null;
        }

        // ChainedRadarPulseTrigger가 풀로 돌아갈 때 호출할 콜백 설정
        trigger.SetReturnCallback(ReturnObjectToPool);
        return trigger;
    }

    public ChainedRadarPulseTrigger GetPooledObject()
    {
        if (!isInitialized)
        {
            Debug.LogError("ChainedPulseObjectPool이 초기화되지 않았습니다. GetPooledObject 호출 불가.", this);
            return null;
        }

        ChainedRadarPulseTrigger obj;
        if (availableObjects.Count > 0)
        {
            obj = availableObjects.Dequeue();
        }
        else
        {
            Debug.LogWarning("풀이 비어있음. 새로운 오브젝트를 생성");
            obj = CreateNewPooledObject();
            if (obj == null) return null;
        }

        obj.transform.SetParent(null);
        obj.gameObject.SetActive(true);
        return obj;
    }

    public void ReturnObjectToPool(ChainedRadarPulseTrigger obj)
    {
        if (obj == null) return;

        if (obj.TargetObject != null && _activePulsingTargets.Contains(obj.TargetObject))
        {
            _activePulsingTargets.Remove(obj.TargetObject);
        }

        if (!obj.gameObject.activeSelf)
        {
            Debug.LogWarning($"오브젝트 '{obj.name}'는 이미 풀에 반환된 상태이거나 비활성화.", obj);
            return;
        }

        obj.gameObject.SetActive(false);
        obj.transform.SetParent(this.transform);
        availableObjects.Enqueue(obj);
    }

    public bool IsTargetPulsing(GameObject targetObject)
    {
        return _activePulsingTargets.Contains(targetObject);
    }

    public void AddActiveTarget(GameObject targetObject)
    {
        if (targetObject == null) return;
        _activePulsingTargets.Add(targetObject);
    }

    public void RemoveActiveTarget(GameObject targetObject)
    {
        if (targetObject == null) return;
        _activePulsingTargets.Remove(targetObject);
    }

    void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }

        foreach (ChainedRadarPulseTrigger trigger in availableObjects)
        {
            if (trigger != null && trigger.gameObject != null)
            {
                Destroy(trigger.gameObject);
            }
        }
        availableObjects.Clear();
        _activePulsingTargets.Clear();
    }
}