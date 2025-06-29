using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Events;

public class PlayerRadarPulseTrigger : MonoBehaviour
{
    // 셰이더 프로퍼티 ID들을 미리 캐싱 
    // static readonly int ->초기화 후 변하지 않는 공유된 정수 값 
    // Shader.PropertyToID->주어진 문자열 이름에 해당하는 고유한 정수 ID를 반환 
    // 이렇게 안 하면 매 코드 실행 시 문자열 파싱 및 해싱해서 성능 상 악영향 
    // 이렇게 하면 준비된 정수 ID를 써서 CPU 부하 줄어 듬 
    // 수백 수천개 작동하는 섀이더 관련 스크립트 사용 시 이렇게 하는 거 권장한다고 함 
    // SHADER_PROP_ -> 이건 일반적으로 쓰이는 세이더 프로퍼티 변수 명명법 
    private static readonly int SHADER_PROP_FADETIME_ID = Shader.PropertyToID("_FadeTime");
    private static readonly int SHADER_PROP_PULSE_COLOR_ID = Shader.PropertyToID("_PulseColor");

    [Header("트리거 설정")]
    [Tooltip("레이더 펄스를 발동시킬 키.")]
    [SerializeField] private KeyCode activationKey = KeyCode.E;
    [Tooltip("레이더 펄스의 쿨타임")]
    [SerializeField] private float cooldownDuration = 2.0f;

    [Header("펄스 속성 설정")]
    [Tooltip("레이더가 커지고 나타나는 총 시간")]
    [SerializeField] private float fadeInDuration = 1.0f;
    [Tooltip("레이더가 완전히 퍼진 후 유지되는 시간")]
    [SerializeField] private float holdDuration = 0f;
    [Tooltip("레이더가 투명해지면서 사라지는 총 시간")]
    [SerializeField] private float fadeOutDuration = 2.0f;

    [Tooltip("레이더의 최종 크기")]
    [SerializeField] private float finalScale = 5f;
    [Tooltip("레이더 펄스의 색상")]
    [SerializeField] private Color pulseColor = Color.cyan;

    [Header("Material 및 Shader 설정")]
    [Tooltip("이 펄스에 사용될 Material")]
    [SerializeField] private Material pulseMat;

    [Header("연쇄 효과 설정")]
    [Tooltip("충돌 시 레이더 발생할 오브젝트의 태그 목록, 정확히 입력")]
    [SerializeField] private List<string> propagationTags = new List<string>();

    [Tooltip("연쇄 펄스 풀에 사용될 프리팹")]
    [SerializeField] private GameObject chainedPulsePrefabForPool;

    [Header("오브젝트 풀 매니저 자동 생성")]
    [Tooltip("씬에 ChainedPulseObjectPool 인스턴스가 없을 경우 자동으로 생성할 프리팹)")]
    [SerializeField] private GameObject chainedPulseObjectPoolManagerPrefab;

    [Header("사운드 설정")]
    [Tooltip("플레이어가 레이더 발생 시 발생할 사운드 효과는 여기서 제어")]
    [SerializeField] private UnityEvent OnPlayerPulseActivated;

    [System.Serializable]
    public class ChainedPulseSpecificSettings
    {
        [Tooltip("이 설정을 적용할 충돌 오브젝트의 태그")]
        public string targetTag;

        [Header("오버라이드 펄스 속성 (선택 사항, -1 이하면 플레이어 값 그대로 사용)")]
        [Tooltip("이 태그에 연쇄되는 펄스의 최종 크기")]
        public float overrideFinalScale = -1f;
        [Tooltip("이 태그에 연쇄되는 펄스의 페이드 인 시간")]
        public float overrideFadeInDuration = -1f;
        [Tooltip("이 태그에 연쇄되는 펄스의 유지 시간")]
        public float overrideHoldDuration = -1f;
        [Tooltip("이 태그에 연쇄되는 펄스의 페이드 아웃 시간")]
        public float overrideFadeOutDuration = -1f;
        [Tooltip("이 태그에 연쇄되는 펄스의 색상 (선택 사항, 알파값 0이면 플레이어 값 그대로 사용) 색 변경시 알파값 숫자 확인!!")]
        public Color overridePulseColor = new Color(0, 0, 0, 0);

        [Header("연쇄 펄스 이벤트")]
        [Tooltip("이 태그를 가진 오브젝트에 연쇄 펄스가 발생했을 때 실행될 이벤트->여기에 사운드 재생 스크립트 넣기")]
        public UnityEvent OnChainedPulseActivatedEvent;
    }

    [Tooltip("태그별 연쇄 펄스 속성 및 이벤트 설정. 목록에 없는 태그는 플레이어 펄스의 기본 속성")]
    [SerializeField] private List<ChainedPulseSpecificSettings> tagSpecificChainedPulseSettings = new List<ChainedPulseSpecificSettings>();

    private bool isCoolingDown = false;
    private float nextReadyTime = 0f;
    private bool isPulsing = false;

    private Renderer pulseRenderer;
    private Material _instanceMaterial;

    void Awake()
    {
        pulseRenderer = GetComponent<Renderer>();
        if (pulseRenderer == null)
        {
            Debug.LogError("Renderer 컴포넌트가 없습니다!", this);
            enabled = false;
            return;
        }

        // Material 인스턴스 생성 
        if (pulseMat != null)
        {
            _instanceMaterial = new Material(pulseMat);
            pulseRenderer.material = _instanceMaterial;
        }
        else if (pulseRenderer.sharedMaterial != null)
        {
            _instanceMaterial = new Material(pulseRenderer.sharedMaterial);
            pulseRenderer.material = _instanceMaterial;
            Debug.LogWarning("Material이 할당되지 않아 Renderer의 Material을 사용", this);
        }
        else
        {
            Debug.LogError("Renderer에 Material이 없음 할당된 pulseMat도 없음. 에러", this);
            enabled = false;
            return;
        }

        transform.localScale = Vector3.zero;

        // 초기 색상 및 페이드 타임 설정 
        if (_instanceMaterial.HasProperty(SHADER_PROP_FADETIME_ID))
        {
            _instanceMaterial.SetFloat(SHADER_PROP_FADETIME_ID, 0f);
        }
        if (_instanceMaterial.HasProperty(SHADER_PROP_PULSE_COLOR_ID))
        {
            _instanceMaterial.SetColor(SHADER_PROP_PULSE_COLOR_ID, pulseColor);
        }

        if (ChainedPulseObjectPool.Instance == null) // 씬에 아직 풀 매니저 인스턴스가 없는 경우
        {
            if (chainedPulseObjectPoolManagerPrefab != null)
            {
                GameObject poolManagerGO = Instantiate(chainedPulseObjectPoolManagerPrefab);
                if (poolManagerGO.GetComponent<ChainedPulseObjectPool>() == null)
                {
                    Debug.LogError("PlayerRadarPulseTrigger:할당된 'Prefab'에 ChainedPulseObjectPool컴포넌트 없음!", this);
                    Destroy(poolManagerGO);
                }
                else
                {
                    Debug.Log("ChainedPulseObjectPoolManager가 씬에 자동으로 생성");
                }
            }
            else
            {
                Debug.LogError("PlayerRadarPulseTrigger:'Prefab'이 할당되지 않음", this);
            }
        }
        // if (chainedPulsePrefabForPool != null && ChainedPulseObjectPool.Instance != null && !ChainedPulseObjectPool.Instance.IsInitialized)
        // {
        //     ChainedPulseObjectPool.Instance.InitializePool(chainedPulsePrefabForPool);
        // }
        // else if (chainedPulsePrefabForPool == null && ChainedPulseObjectPool.Instance != null && !ChainedPulseObjectPool.Instance.IsInitialized)
        // {
        //     Debug.LogError("chainedPulsePrefabForPool이 PlayerRadarPulseTrigger에 할당되지 않음", this);
        // }
    }

    void Update()
    {
        // 쿨타임 중이거나 아직 쿨타임이 끝나지 않았다면 입력 감지 안함 
        if (isCoolingDown && Time.time < nextReadyTime)
        {
            return;
        }

        if (isCoolingDown && Time.time >= nextReadyTime)
        {
            isCoolingDown = false;
        }

        if (Input.GetKeyDown(activationKey) && !isCoolingDown && !isPulsing) // isPulsing 추가 
        {
            StartCoroutine(PulseEffectRoutine());
        }
    }

    public void ActivatePulse()
    {
        if (!isCoolingDown && !isPulsing) // isPulsing 추가 
        {
            StartCoroutine(PulseEffectRoutine());
        }
    }

    IEnumerator PulseEffectRoutine()
    {
        isPulsing = true;
        isCoolingDown = true;
        nextReadyTime = Time.time + cooldownDuration;

        // 플레이어 발동 사운드 이벤트 호출 
        OnPlayerPulseActivated?.Invoke();

        float time = 0f;
        Vector3 startLocalScale = transform.localScale;
        Vector3 targetLocalScale = new Vector3(finalScale, finalScale, finalScale);

        // 페이드 인 
        while (time < fadeInDuration)
        {
            time += Time.deltaTime;
            float t = Mathf.Clamp01(time / fadeInDuration);

            transform.localScale = Vector3.Lerp(startLocalScale, targetLocalScale, t);
            if (_instanceMaterial.HasProperty(SHADER_PROP_FADETIME_ID))
            {
                _instanceMaterial.SetFloat(SHADER_PROP_FADETIME_ID, t);
            }
            if (_instanceMaterial.HasProperty(SHADER_PROP_PULSE_COLOR_ID))
            {
                _instanceMaterial.SetColor(SHADER_PROP_PULSE_COLOR_ID, pulseColor);
            }
            yield return null;
        }

        //유지 시간 
        transform.localScale = targetLocalScale;
        if (_instanceMaterial.HasProperty(SHADER_PROP_FADETIME_ID))
        {
            _instanceMaterial.SetFloat(SHADER_PROP_FADETIME_ID, 1.0f);
        }
        if (_instanceMaterial.HasProperty(SHADER_PROP_PULSE_COLOR_ID))
        {
            _instanceMaterial.SetColor(SHADER_PROP_PULSE_COLOR_ID, pulseColor);
        }

        if (holdDuration > 0)
        {
            yield return new WaitForSeconds(holdDuration);
        }

        //투명해짐 (페이드 아웃) 
        time = 0f;
        while (time < fadeOutDuration)
        {
            time += Time.deltaTime;
            float t = Mathf.Clamp01(time / fadeOutDuration);
            if (_instanceMaterial.HasProperty(SHADER_PROP_FADETIME_ID))
            {
                _instanceMaterial.SetFloat(SHADER_PROP_FADETIME_ID, 1.0f - t);
            }
            if (_instanceMaterial.HasProperty(SHADER_PROP_PULSE_COLOR_ID))
            {
                _instanceMaterial.SetColor(SHADER_PROP_PULSE_COLOR_ID, pulseColor);
            }
            yield return null;
        }

        if (_instanceMaterial.HasProperty(SHADER_PROP_FADETIME_ID))
        {
            _instanceMaterial.SetFloat(SHADER_PROP_FADETIME_ID, 0.0f); 
        }
        if (_instanceMaterial.HasProperty(SHADER_PROP_PULSE_COLOR_ID))
        {
            _instanceMaterial.SetColor(SHADER_PROP_PULSE_COLOR_ID, pulseColor);
        }

        transform.localScale = Vector3.zero;
        isPulsing = false; 
    }

    void OnTriggerEnter(Collider other)
    {
        ChainedRadarPulseTrigger existingChainedTrigger = other.GetComponent<ChainedRadarPulseTrigger>();
        if (existingChainedTrigger != null && existingChainedTrigger.IsPulsing)
        {
            // Debug.Log($"대상 오브젝트 '{other.name}'에 이미 연쇄 펄스가 진행 중. 중복 발생 방지.", other);
            return;
        }
        if (!isPulsing || (existingChainedTrigger != null && existingChainedTrigger.IsPulsing))
        {
            return;
        }

        // activeInHierarchy=활성화 유무 체크 
        if (!other.gameObject.activeInHierarchy)
        {
            return;
        }

        // 싱글톤 인스턴스 사용 및 안전 널체크 
        if (ChainedPulseObjectPool.Instance == null || !ChainedPulseObjectPool.Instance.IsInitialized)
        {
            Debug.LogWarning("ChainedPulseObjectPool이 초기화되지 않았거나 인스턴스가 없습니다. 연쇄 펄스 스킵.", this);
            return;
        }

        // other.gameObject가 현재 이미 연쇄 펄스를 발동 중인지 확인
        if (ChainedPulseObjectPool.Instance.IsTargetPulsing(other.gameObject))
        {
            // Debug.Log($"대상 오브젝트 '{other.name}'에 대한 연쇄 펄스가 이미 기록되어 진행 중. 중복 발생 방지.", other);
            return;
        }

        // propagationTags에 포함된 태그인지 확인 
        bool isInPropagationList = false;
        if (propagationTags != null)
        {
            foreach (string tag in propagationTags)
            {
                if (other.CompareTag(tag))
                {
                    isInPropagationList = true;
                    break;
                }
            }
        }

        if (isInPropagationList)
        {
            // 풀에서 오브젝트를 가져옴 
            ChainedRadarPulseTrigger newChainedTrigger = ChainedPulseObjectPool.Instance.GetPooledObject();

            if (newChainedTrigger != null)
            {
                ChainedPulseObjectPool.Instance.AddActiveTarget(other.gameObject);
                newChainedTrigger.transform.position = other.transform.position;
                newChainedTrigger.transform.rotation = Quaternion.identity;

                // 충돌한 오브젝트의 태그를 기반으로 설정 찾기 
                ChainedPulseSpecificSettings foundSettings = null;
                foreach (var setting in tagSpecificChainedPulseSettings)
                {
                    if (other.CompareTag(setting.targetTag))
                    {
                        foundSettings = setting;
                        break;
                    }
                }

                // 펄스 속성 설정: 태그별 설정이 있으면 오버라이드, 없으면 플레이어 펄스의 기본 속성 사용 
                float currentFinalScale = (foundSettings != null && foundSettings.overrideFinalScale >= 0) ? foundSettings.overrideFinalScale : finalScale;
                float currentFadeInDuration = (foundSettings != null && foundSettings.overrideFadeInDuration >= 0) ? foundSettings.overrideFadeInDuration : fadeInDuration;
                float currentHoldDuration = (foundSettings != null && foundSettings.overrideHoldDuration >= 0) ? foundSettings.overrideHoldDuration : holdDuration;
                float currentFadeOutDuration = (foundSettings != null && foundSettings.overrideFadeOutDuration >= 0) ? foundSettings.overrideFadeOutDuration : fadeOutDuration;
                Color currentPulseColor = (foundSettings != null && foundSettings.overridePulseColor.a > 0.001f) ? foundSettings.overridePulseColor : pulseColor;

                newChainedTrigger.SetPulseParameters(
                    currentFadeInDuration,
                    currentHoldDuration,
                    currentFadeOutDuration,
                    currentFinalScale,
                    currentPulseColor,
                    other.gameObject // 이 연쇄 펄스를 발동시킨 대상 오브젝트 전달
                );

                newChainedTrigger.ActivatePulse(); // 연쇄 펄스 활성화 

                // 태그별 이벤트 호출 
                foundSettings?.OnChainedPulseActivatedEvent?.Invoke();
            }
        }
    }

    void OnDestroy()
    {
        if (_instanceMaterial != null)
        {
            Destroy(_instanceMaterial);
        }
    }
}