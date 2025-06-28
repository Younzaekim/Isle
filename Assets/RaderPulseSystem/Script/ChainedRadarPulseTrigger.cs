using UnityEngine;
using System.Collections;
using System;

public class ChainedRadarPulseTrigger : MonoBehaviour
{
    private static readonly int SHADER_PROP_FADETIME_ID = Shader.PropertyToID("_FadeTime");
    private static readonly int SHADER_PROP_PULSE_COLOR_ID = Shader.PropertyToID("_PulseColor");

    private float _fadeInDuration; 
    private float _holdDuration; // 더 이상 사용되지 않음
    private float _fadeOutDuration; // 더 이상 사용되지 않음
    private float _finalScale;
    private Color _pulseColor;

    private Renderer _pulseRenderer;
    private Material _instanceMaterial;

    private Action<ChainedRadarPulseTrigger> _returnToPoolCallback;
    private bool _isPulsing = false;
    private Coroutine _currentPulseRoutine;
    public GameObject TargetObject { get; private set; }

    public bool IsPulsing => _isPulsing;

    void Awake()
    {
        _pulseRenderer = GetComponent<Renderer>();
        if (_pulseRenderer == null)
        {
            Debug.LogError("ChainedRadarPulseTrigger: Renderer 컴포넌트가 없습니다!", this);
            enabled = false;
            return;
        }

        if (_pulseRenderer.sharedMaterial != null)
        {
            _instanceMaterial = new Material(_pulseRenderer.sharedMaterial);
            _pulseRenderer.material = _instanceMaterial;
        }
        else
        {
            Debug.LogError("ChainedRadarPulseTrigger: Renderer에 할당된 Material이 없습니다. 에러!", this);
            enabled = false;
            return;
        }

        // 초기 상태 설정 
        InitializeVisualState();
    }

    // 오브젝트가 활성화될 때마다 초기화 로직 수행 
    void OnEnable()
    {
        InitializeStateForPooling();
        InitializeVisualState();
    }

    private void InitializeStateForPooling()
    {
        if (_currentPulseRoutine != null)
        {
            StopCoroutine(_currentPulseRoutine);
            _currentPulseRoutine = null;
        }
        _isPulsing = false;
        TargetObject = null; //풀로 돌아올 때 대상 오브젝트 정보 초기화
    }

    private void InitializeVisualState()
    {
        transform.localScale = Vector3.zero;
        if (_instanceMaterial.HasProperty(SHADER_PROP_FADETIME_ID))
        {
            _instanceMaterial.SetFloat(SHADER_PROP_FADETIME_ID, 0f);
        }
        if (_instanceMaterial.HasProperty(SHADER_PROP_PULSE_COLOR_ID))
        {
            _instanceMaterial.SetColor(SHADER_PROP_PULSE_COLOR_ID, Color.clear);
        }
    }

    public void SetReturnCallback(Action<ChainedRadarPulseTrigger> callback)
    {
        _returnToPoolCallback = callback;
    }

    public void SetPulseParameters(float fadeInDuration, float holdDuration, float fadeOutDuration, float finalScale, Color pulseColor, GameObject targetObject)
    {
        _fadeInDuration = fadeInDuration; 
        _holdDuration = holdDuration; // 더 이상 사용되지 않음
        _fadeOutDuration = fadeOutDuration; // 더 이상 사용되지 않음
        _finalScale = finalScale;
        _pulseColor = pulseColor;
        TargetObject = targetObject;
    }

    public void ActivatePulse()
    {
        if (!_isPulsing)
        {
            _currentPulseRoutine = StartCoroutine(PulseEffectRoutine());
        }
        else
        {
            //Debug.LogWarning($"ChainedRadarPulseTrigger: 오브젝트 '{gameObject.name}'는 이미 펄스 효과가 진행 중입니다. 중복 활성화 방지.", this);
        }
    }

    IEnumerator PulseEffectRoutine()
    {
        _isPulsing = true;

        float totalAnimationDuration = _fadeInDuration; 
        float time = 0f;
        Vector3 startLocalScale = transform.localScale;
        Vector3 targetLocalScale = new Vector3(_finalScale, _finalScale, _finalScale);

        if (_instanceMaterial.HasProperty(SHADER_PROP_PULSE_COLOR_ID))
        {
            _instanceMaterial.SetColor(SHADER_PROP_PULSE_COLOR_ID, _pulseColor);
        }

        while (time < totalAnimationDuration)
        {
            if (!_isPulsing) yield break; // 펄스가 중단되면 코루틴 종료 

            time += Time.deltaTime;
            float t = Mathf.Clamp01(time / totalAnimationDuration); 

            transform.localScale = Vector3.Lerp(startLocalScale, targetLocalScale, t);
            if (_instanceMaterial.HasProperty(SHADER_PROP_FADETIME_ID))
            {
                _instanceMaterial.SetFloat(SHADER_PROP_FADETIME_ID, 1.0f - t);
            }
            yield return null;
        }


        FinishPulse();
    }

    private void FinishPulse()
    {
        _isPulsing = false;
        InitializeVisualState();
        _currentPulseRoutine = null;

        _returnToPoolCallback?.Invoke(this); // 풀로 반환 
    }

    void OnDisable()
    {
        InitializeStateForPooling();
        InitializeVisualState();
    }

    void OnDestroy()
    {
        if (_instanceMaterial != null)
        {
            Destroy(_instanceMaterial);
        }
    }
}