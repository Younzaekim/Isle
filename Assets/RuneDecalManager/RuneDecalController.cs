using UnityEngine;
using UnityEngine.Rendering.Universal;
using System.Collections;

[ExecuteInEditMode]
public class RuneDecalController : MonoBehaviour
{
    [Header("플레이어")]
    private Transform player;
    private float triggerDistance = 3f; 

    [Header("데칼 머티리얼 및 텍스처")]
    [SerializeField] private DecalProjector decalProjector;
    [SerializeField] private Material decalMaterial;
    [SerializeField] private Texture2D runeTexture;

    [Header("이펙트 세부 조정")]
    [SerializeField] private float alphaFadeInDuration = 0.5f;
    [SerializeField] private float emissionFadeSpeed = 2f;
    [SerializeField] private float maxEmission = 2f;
    [SerializeField] private Color activeEmissionColor = Color.cyan;

    private bool revealOnApproach = false; // 이 변수는 이제 직접적인 초기 알파 설정에는 사용되지 않음.
    private float revealDelay = 0f; // 이 변수는 현재 코드에서 사용되지 않음

    [Header("초기 상태 설정")] 
    [Tooltip("시작 시 또는 에디터 미리보기에서 데칼의 초기 알파 값")]
    [Range(0f, 1f)]
    [SerializeField] private float initialAlpha = 1f;

    public float InitialAlpha => initialAlpha;
    private bool _isRevealed = false; // 알파 페이드인이 완료되어 완전히 나타났는지 여부
    public bool IsRevealed => _isRevealed; // 외부에서 접근 가능하도록 프로퍼티

    private float _currentEmissionValue = 0f;
    public float CurrentEmissionValue => _currentEmissionValue; // 외부에서 현재 이미션 값 확인용 프로퍼티

    private Coroutine _currentEffectCoroutine; // 알파/이미션 페이드인/아웃 코루틴 참조
    private Coroutine _flashCoroutine; // 이미션 플래시 코루틴 참조

    private Material _runtimeDecalMaterialInstance; // 런타임에 사용할 Material 인스턴스
    private Color _originalEmissionColor;           // Material의 원래 이미션 색상

    //세이더 프로퍼티 값들
    private const string SHADER_PROP_BASEMAP = "_BaseMap";
    private const string SHADER_PROP_BASECOLOR = "_BaseColor";
    private const string SHADER_PROP_EMISSION = "_Emission";
    private const string SHADER_PROP_EMISSION_COLOR = "_EmissionColor";


    void Start()
    {
        if (Application.isEditor && !Application.isPlaying)
        {
            InitializeDecalInEditMode();
            return;
        }

        InitializeDecalRuntime();
    }

    // 에디터 모드에서 데칼을 초기화하고 미리보기 적용
    private void InitializeDecalInEditMode()
    {
        if (decalProjector == null || decalMaterial == null) return;

        // Material 인스턴스 생성 또는 재활용 (에디터에서는 DestroyImmediate 사용)
        if (_runtimeDecalMaterialInstance == null || _runtimeDecalMaterialInstance.name != decalMaterial.name + "(Clone)")
        {
            if (_runtimeDecalMaterialInstance != null) DestroyImmediate(_runtimeDecalMaterialInstance);
            _runtimeDecalMaterialInstance = new Material(decalMaterial);
            _runtimeDecalMaterialInstance.name = decalMaterial.name + "(Clone)";
        }

        decalProjector.material = _runtimeDecalMaterialInstance;

        ApplyRuneTexture(); // 할당된 룬 텍스처를 Material에 적용

        // 에디터에서는 initialAlpha 값을 따르도록 변경
        Color baseColor = _runtimeDecalMaterialInstance.GetColor(SHADER_PROP_BASECOLOR);
        baseColor.a = initialAlpha; // <--- initialAlpha 값 적용
        _runtimeDecalMaterialInstance.SetColor(SHADER_PROP_BASECOLOR, baseColor);
        _runtimeDecalMaterialInstance.SetFloat(SHADER_PROP_EMISSION, 0f);
        // _originalEmissionColor는 런타임에서 초기화되지만, 에디터 미리보기에 영향 주지 않음
    }

    // 런타임 시 데칼 초기화
    private void InitializeDecalRuntime()
    {
        if (decalProjector == null || decalMaterial == null)
        {
            Debug.LogError(gameObject.name + ": Decal Projector/Material이 할당되지 않았습니다!!", this);
            enabled = false;
            return;
        }

        // Material 인스턴스 생성 및 할당 (런타임에서는 일반 Destroy 사용)
        if (_runtimeDecalMaterialInstance != null) Destroy(_runtimeDecalMaterialInstance);
        _runtimeDecalMaterialInstance = new Material(decalMaterial);
        decalProjector.material = _runtimeDecalMaterialInstance;

        ApplyRuneTexture(); // 룬 텍스처 적용

        // 런타임 시작 시 initialAlpha 값에 따라 알파 설정
        Color baseColor = _runtimeDecalMaterialInstance.GetColor(SHADER_PROP_BASECOLOR);
        baseColor.a = initialAlpha; // <--- initialAlpha 값 적용
        _runtimeDecalMaterialInstance.SetColor(SHADER_PROP_BASECOLOR, baseColor);

        _originalEmissionColor = _runtimeDecalMaterialInstance.GetColor(SHADER_PROP_EMISSION_COLOR);
        _runtimeDecalMaterialInstance.SetFloat(SHADER_PROP_EMISSION, 0f);

        // _isRevealed 초기 상태도 initialAlpha 값에 따라 설
        _isRevealed = initialAlpha > 0.01f; // 0.01f보다 크면 revealed 상태로 간주
        enabled = false; // RuneSequenceManager가 제어할 때까지 이 스크립트의 Update() 등은 비활성화
    }

    // runeTexture를 Material의 _BaseMap에 적용
    private void ApplyRuneTexture()
    {
        if (_runtimeDecalMaterialInstance != null)
        {
            if (runeTexture != null)
            {
                _runtimeDecalMaterialInstance.SetTexture(SHADER_PROP_BASEMAP, runeTexture);
            }
            else
            {
                // 텍스처가 없는 경우, Material의 기본 Base Map을 사용하거나 null로 설정
                _runtimeDecalMaterialInstance.SetTexture(SHADER_PROP_BASEMAP, null);
            }
        }
    }

    // 에디터에서 인스펙터 값이 변경될 때 호출
    void OnValidate()
    {
        if (decalProjector == null || decalMaterial == null) return;

        // 에디터 모드에서만 실행되도록
        if (Application.isEditor && !Application.isPlaying)
        {
            InitializeDecalInEditMode(); // 텍스처 및 초기 알파 미리보기를 위해 초기화 재호출
        }
    }

    // 1단계: 지정된 시간 동안 알파와 이미션을 함께 페이드인
    public IEnumerator RevealAndGlow(float fadeInDuration, float maxEmit, Color emitColor)
    {
        if (_currentEffectCoroutine != null) StopCoroutine(_currentEffectCoroutine); // 기존 코루틴 중지
        _currentEffectCoroutine = StartCoroutine(RevealAndGlowEffectInternal(fadeInDuration, maxEmit, emitColor));
        yield return _currentEffectCoroutine; // 이 코루틴이 끝날 때까지 대기
    }

    private IEnumerator RevealAndGlowEffectInternal(float fadeInDuration, float maxEmit, Color emitColor)
    {
        _isRevealed = false; // 시작 시 아직 완전히 나타나지 않음

        Color currentBaseColor = _runtimeDecalMaterialInstance.GetColor(SHADER_PROP_BASECOLOR);
        float startAlpha = currentBaseColor.a;
        float targetAlpha = 1f;

        float startEmission = _currentEmissionValue;
        float targetEmission = maxEmit;
        Color startEmissionColor = _runtimeDecalMaterialInstance.GetColor(SHADER_PROP_EMISSION_COLOR);
        Color targetEmissionColor = emitColor;

        float timer = 0f;
        while (timer < 1f)
        {
            timer += Time.deltaTime / fadeInDuration;

            float alpha = Mathf.Lerp(startAlpha, targetAlpha, timer);
            currentBaseColor.a = alpha;
            _runtimeDecalMaterialInstance.SetColor(SHADER_PROP_BASECOLOR, currentBaseColor);

            // 이미션은 알파 페이드인 속도와 독립적으로 emissionFadeSpeed에 따라 조절
            float emissionLerpT = Mathf.Min(1f, timer * (fadeInDuration * emissionFadeSpeed)); 
            float emissionTimer = timer; // 알파와 동일한 타이머 사용
            _currentEmissionValue = Mathf.Lerp(startEmission, targetEmission, emissionTimer);
            Color interpolatedEmissionColor = Color.Lerp(startEmissionColor, targetEmissionColor, emissionTimer);

            _runtimeDecalMaterialInstance.SetFloat(SHADER_PROP_EMISSION, _currentEmissionValue);
            _runtimeDecalMaterialInstance.SetColor(SHADER_PROP_EMISSION_COLOR, interpolatedEmissionColor);

            yield return null;
        }

        // 최종 값 보장
        currentBaseColor.a = targetAlpha;
        _runtimeDecalMaterialInstance.SetColor(SHADER_PROP_BASECOLOR, currentBaseColor);
        _currentEmissionValue = targetEmission;
        _runtimeDecalMaterialInstance.SetFloat(SHADER_PROP_EMISSION, targetEmission);
        _runtimeDecalMaterialInstance.SetColor(SHADER_PROP_EMISSION_COLOR, targetEmissionColor);

        _isRevealed = true; // 알파 페이드인 완료!
    }

    // 2/3단계: 이미션만 빠르게 반짝였다가 0으로 페이드아웃
    public IEnumerator FlashAndFadeEmission(float peakEmission, float flashDuration, Color flashColor)
    {
        if (_flashCoroutine != null) StopCoroutine(_flashCoroutine); // 기존 플래시 코루틴 중지
        _flashCoroutine = StartCoroutine(FlashAndFadeEmissionInternal(peakEmission, flashDuration, flashColor));
        yield return _flashCoroutine; // 이 코루틴이 끝날 때까지 대기
    }

    private IEnumerator FlashAndFadeEmissionInternal(float peakEmission, float flashDuration, Color flashColor)
    {
        float startEmission = _currentEmissionValue; // 현재 이미션 값에서 시작
        Color startEmissionColor = _runtimeDecalMaterialInstance.GetColor(SHADER_PROP_EMISSION_COLOR);

        // 먼저 peakEmission으로 빠르게 도달 (총 지속 시간의 절반)
        float timer = 0f;
        float halfDuration = flashDuration * 0.5f;
        while (timer < halfDuration)
        {
            timer += Time.deltaTime;
            _currentEmissionValue = Mathf.Lerp(startEmission, peakEmission, timer / halfDuration);
            _runtimeDecalMaterialInstance.SetFloat(SHADER_PROP_EMISSION, _currentEmissionValue);
            _runtimeDecalMaterialInstance.SetColor(SHADER_PROP_EMISSION_COLOR, Color.Lerp(startEmissionColor, flashColor, timer / halfDuration));
            yield return null;
        }
        _currentEmissionValue = peakEmission; // 정확한 피크 값 설정
        _runtimeDecalMaterialInstance.SetFloat(SHADER_PROP_EMISSION, peakEmission);
        _runtimeDecalMaterialInstance.SetColor(SHADER_PROP_EMISSION_COLOR, flashColor);


        // 이제 0으로 페이드아웃
        startEmission = _currentEmissionValue;
        startEmissionColor = _runtimeDecalMaterialInstance.GetColor(SHADER_PROP_EMISSION_COLOR);
        timer = 0f;
        while (timer < halfDuration)
        {
            timer += Time.deltaTime;
            _currentEmissionValue = Mathf.Lerp(startEmission, 0f, timer / halfDuration);
            _runtimeDecalMaterialInstance.SetFloat(SHADER_PROP_EMISSION, _currentEmissionValue);
            _runtimeDecalMaterialInstance.SetColor(SHADER_PROP_EMISSION_COLOR, Color.Lerp(startEmissionColor, _originalEmissionColor, timer / halfDuration));
            yield return null;
        }
        _currentEmissionValue = 0f;
        _runtimeDecalMaterialInstance.SetFloat(SHADER_PROP_EMISSION, 0f);
        _runtimeDecalMaterialInstance.SetColor(SHADER_PROP_EMISSION_COLOR, _originalEmissionColor);
    }

    // 이미션 페이드 인/아웃을 처리 매서드
    public IEnumerator FadeEmission(float targetEmission, Color targetEmissionColor)
    {
        if (_flashCoroutine != null) StopCoroutine(_flashCoroutine);
        if (_currentEffectCoroutine != null) StopCoroutine(_currentEffectCoroutine);

        float startEmission = _currentEmissionValue;
        Color startEmissionColor = _runtimeDecalMaterialInstance.GetColor(SHADER_PROP_EMISSION_COLOR);
        float timer = 0f;

        while (timer < 1f)
        {
            timer += Time.deltaTime * emissionFadeSpeed;
            _currentEmissionValue = Mathf.Lerp(startEmission, targetEmission, timer);
            Color interpolatedColor = Color.Lerp(startEmissionColor, targetEmissionColor, timer);

            _runtimeDecalMaterialInstance.SetFloat(SHADER_PROP_EMISSION, _currentEmissionValue);
            _runtimeDecalMaterialInstance.SetColor(SHADER_PROP_EMISSION_COLOR, interpolatedColor);

            yield return null;
        }

        _currentEmissionValue = targetEmission;
        _runtimeDecalMaterialInstance.SetFloat(SHADER_PROP_EMISSION, targetEmission);
        _runtimeDecalMaterialInstance.SetColor(SHADER_PROP_EMISSION_COLOR, targetEmissionColor);
    }

    //Material의 _BaseColor 알파값 조절 매서드
    public void SetMaterialAlpha(float alpha)
    {
        if (_runtimeDecalMaterialInstance != null)
        {
            Color currentBaseColor = _runtimeDecalMaterialInstance.GetColor(SHADER_PROP_BASECOLOR);
            currentBaseColor.a = alpha;
            _runtimeDecalMaterialInstance.SetColor(SHADER_PROP_BASECOLOR, currentBaseColor);
        }
    }

    void OnDestroy()
    {
        //에디터 모드에서 Material 인스턴스가 남아있지 않도록 정리
        if (_runtimeDecalMaterialInstance != null)
        {
            if (Application.isEditor)
            {
                DestroyImmediate(_runtimeDecalMaterialInstance);
            }
            else
            {
                Destroy(_runtimeDecalMaterialInstance);
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        if (decalProjector != null)
        {
            Gizmos.color = Color.blue;
            Matrix4x4 originalMatrix = Gizmos.matrix;
            Gizmos.matrix = decalProjector.transform.localToWorldMatrix;

            Gizmos.DrawWireCube(decalProjector.pivot - new Vector3(0, 0, decalProjector.size.z * 0.5f), decalProjector.size);

            Gizmos.matrix = originalMatrix;
        }
    }
}