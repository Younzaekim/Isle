using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Events;
using UnityEngine.Rendering.Universal;

public class RuneSequenceManager : MonoBehaviour
{
    [Header("플레이어 설정")]
    [SerializeField] private Transform player;
    [SerializeField] private float triggerDistance = 3f;

    [Header("룬 데칼 시퀀스")]
    [Tooltip("순서대로 'L,A,P,U,T,A'와 같이 룬 데칼 컨트롤러를 할당하세요.")]
    [SerializeField] private RuneDecalController[] runeDecals;

    [Header("단계 전환 딜레이")]
    [SerializeField] private float delayAfterStage1 = 1.0f; // 1단계 완료 후 2단계 시작 전 딜레이
    [SerializeField] private float delayAfterStage2 = 1.0f; // 2단계 완료 후 3단계 시작 전 딜레이

    [Header("1단계: 개별 문자 등장 설정")]
    [Tooltip("각 룬 문자가 나타나는 초기 딜레이 (초). 배열 크기는 runeDecals와 같아야 합니다.")]
    [SerializeField] private float[] individualRevealDelays;          // 각 문자의 초기 등장 딜레이 시간
    [SerializeField] private float individualFadeInDuration = 0.5f; // 각 문자의 알파 페이드인 시간
    [SerializeField] private float individualMaxEmission = 8f; // 각 문자의 최대 발광 강도
    [SerializeField] private Color individualEmissionColor = Color.cyan; // 각 문자의 발광 색상

    [Header("2단계: 전체 하이라이트 설정")]
    [SerializeField] private float overallFlashPeakEmission = 15f; // 모든 문자가 동시에 반짝일 때의 피크 발광 강도
    [SerializeField] private float overallFlashDuration = 0.5f; // 전체 플래시 지속 시간 (피크 도달 및 복귀)
    [SerializeField] private Color overallFlashColor = Color.white; // 전체 플래시 색상

    [Header("3단계: 순차적 하이라이트 설정")]
    [SerializeField] private float sequentialFlashPeakEmission = 20f; // 순차적으로 반짝일 때의 피크 발광 강도 (더 강하게)
    [SerializeField] private float sequentialFlashDuration = 0.8f; // 순차적 플래시 지속 시간 (피크 도달 및 0으로 페이드아웃)
    [SerializeField] private float sequentialFlashDelay = 0.1f; // 각 문자가 순차적으로 반짝이는 사이의 딜레이

    [Header("리셋 페이드아웃 설정")]
    [SerializeField] private float resetFadeOutDuration = 0.5f; // 리셋 시 알파 페이드 아웃 시간


    // 각 단계 시작 시 호출될 Action 이벤트 정의 (코드에서 구독)->사운드 재생할 때 사용할 용도!
    public static event System.Action OnStage1Start;            // 1단계 시작 시
    public static event System.Action OnStage2Start;            // 2단계 시작 시
    public static event System.Action OnStage3Start;            // 3단계 시작 시
    public static event System.Action OnSequenceCompleted; // 전체 시퀀스 완료 시
    public static event System.Action OnResetStart;            // 리셋 시작 시


    [Header("시스템 연동 이벤트")]
    [Tooltip("룬 시퀀스 3단계가 모두 완료되었을 때 호출될 UnityEvent입니다.")]
    public UnityEvent OnSequenceCompletedByUnityEvent; // 3단계 완료 시 유니티 에디터에서 함수 연결 가능


    private bool _playerInRange = false;
    private bool _sequenceStarted = false;
    private bool _sequenceCompletedPermanently = false; // 시퀀스가 최종적으로 완료되어 더 이상 활성화되지 않음
    private Coroutine _currentSequenceCoroutine;
    private List<Coroutine> _runeEffectCoroutines = new List<Coroutine>(); // 룬 효과 코루틴 추적

    void Start()
    {
        if (player == null)
        {
            GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
            if (playerObject != null)
            {
                player = playerObject.transform;
                Debug.Log("RuneSequenceManager:'Player'태그로 찾음" + player.name);
            }
            else
            {
                Debug.LogError("RuneSequenceManager:'Player'태그로 못 찾음", this);
                enabled = false;
                return;
            }
        }
        //룬 데칼 인스펙터에서 직접 할당
        if (runeDecals == null || runeDecals.Length == 0)
        {
            Debug.LogError("Rune Decals 배열이 비어있습니다!룬 데칼 컨트롤러를 할당해주세요.", this);
            enabled = false;
            return;
        }

        //배열 크기 검사 및 초기화
        if (individualRevealDelays == null || individualRevealDelays.Length != runeDecals.Length)
        {
            Debug.LogWarning("Individual Reveal Delays 배열의 크기가 Rune Decals 배열과 일치하지 않습니다. 자동으로 초기화합니다.", this);
            individualRevealDelays = new float[runeDecals.Length];
            for (int i = 0; i < individualRevealDelays.Length; i++)
            {
                individualRevealDelays[i] = i * 0.2f; // 초기값
            }
        }

        // 각 RuneDecalController 초기 설정
        foreach (RuneDecalController decal in runeDecals)
        {
            if (decal != null)
            {
                decal.enabled = false; // RuneDecalController 비활성화 (데칼이 Update를 돌지 않게 함)
            }
        }
    }

    void Update()
    {
        // 시퀀스가 3단계까지 완료시 리턴
        if (_sequenceCompletedPermanently)
        {
            return;
        }

        float distance = Vector3.Distance(player.position, transform.position);

        if (distance < triggerDistance)
        {
            if (!_playerInRange)
            {
                _playerInRange = true;
                if (!_sequenceStarted)
                {
                    _sequenceStarted = true;
                    if (_currentSequenceCoroutine != null) StopCoroutine(_currentSequenceCoroutine);
                    _currentSequenceCoroutine = StartCoroutine(RuneActivationSequence());
                }
            }
        }
        else
        {
            if (_playerInRange)
            {
                _playerInRange = false;
                // _sequenceStarted = false; //리셋 완료 후 _sequenceStarted = false; 로 설정됨

                if (!_sequenceCompletedPermanently) // 영구 완료되지 않았다면 리셋 가능
                {
                    if (_currentSequenceCoroutine != null) StopCoroutine(_currentSequenceCoroutine);
                    _currentSequenceCoroutine = StartCoroutine(ResetRunes());
                }
            }
        }
    }

    // 룬 활성화 시퀀스 전체를 관리하는 코루틴
    private IEnumerator RuneActivationSequence()
    {
        Debug.Log("룬 활성화 시퀀스 시작!");

        // 기존 룬 효과 코루틴이 남아있을 경우 대비하여 중지
        StopAllRuneEffectCoroutines();

        // 1단계 시작
        OnStage1Start?.Invoke();
        yield return StartCoroutine(Stage1_IndividualReveal());
        // 모든 룬이 실제로 드러났는지 확인 (initialAlpha가 0이었다가 RevealAndGlow로 1이 되는 경우)
        yield return new WaitUntil(() => AreAllRunesRevealed());
        Debug.Log("1단계 완료: 모든 룬 문자 나타남.");

        yield return new WaitForSeconds(delayAfterStage1);

        // 2단계 시작
        OnStage2Start?.Invoke();
        yield return StartCoroutine(Stage2_OverallFlash());
        Debug.Log("2단계 완료: 전체 룬 하이라이트.");
        _sequenceCompletedPermanently = true;
        yield return new WaitForSeconds(delayAfterStage2); // 2단계와 3단계 사이의 딜레이 추가

        // 3단계 시작
        OnStage3Start?.Invoke();
        yield return StartCoroutine(Stage3_SequentialFlashAndFade());
        Debug.Log("3단계 완료: 순차적 하이라이트 및 페이드아웃.");

        // 3단계가 최종 완료되었을 때만 영구 완료 상태로 설정
        OnSequenceCompletedByUnityEvent?.Invoke(); // 시퀀스 완료했다고 유니티 이벤트로 알림
        Debug.Log("룬 활성화 시퀀스 최종 완료.");

        // 전체 시퀀스 완료 사운드 이벤트
        OnSequenceCompleted?.Invoke();

        foreach (RuneDecalController decal in runeDecals)
        {
            if (decal != null)
            {
                // RuneDecalController의 Update가 더 이상 돌지 않도록 비활성화
                decal.enabled = false;
            }
        }
    }

    // 모든 룬 효과 코루틴을 중지하는 메서드
    private void StopAllRuneEffectCoroutines()
    {
        foreach (var coroutine in _runeEffectCoroutines)
        {
            if (coroutine != null) StopCoroutine(coroutine);
        }
        _runeEffectCoroutines.Clear();
    }


    //모든 룬 나타났는지 확인용 매서드
    private bool AreAllRunesRevealed()
    {
        foreach (RuneDecalController decal in runeDecals)
        {
            // decal이 null이 아니고, decal이 현재 완전히 드러나지 않았다면 false 반환
            if (decal == null || !decal.IsRevealed)
            {
                return false;
            }
        }
        return true;
    }

    private IEnumerator Stage1_IndividualReveal()
    {
        StopAllRuneEffectCoroutines(); // 새 단계 시작 시 기존 코루틴 목록 초기화
        for (int i = 0; i < runeDecals.Length; i++)
        {
            RuneDecalController decal = runeDecals[i];
            if (decal != null)
            {
                decal.enabled = true; // 룬 데칼 컨트롤러 활성화
                Coroutine revealCoroutine = StartCoroutine(
                    DelayedReveal(decal, individualRevealDelays[i], individualFadeInDuration, individualMaxEmission, individualEmissionColor)
                );
                _runeEffectCoroutines.Add(revealCoroutine);
            }
        }
        // 코루틴 종료 대기
        foreach (Coroutine coroutine in _runeEffectCoroutines)
        {
            if (coroutine != null) yield return coroutine;
        }
    }

    private IEnumerator DelayedReveal(RuneDecalController decal, float delay, float fadeInDuration, float maxEmit, Color emitColor)
    {
        yield return new WaitForSeconds(delay);
        yield return decal.RevealAndGlow(fadeInDuration, maxEmit, emitColor);
    }

    private IEnumerator Stage2_OverallFlash()
    {
        StopAllRuneEffectCoroutines(); // 새 단계 시작 시 기존 코루틴 목록 초기화
        List<Coroutine> flashCoroutines = new List<Coroutine>(); // 이 단계의 코루틴만 추적
        foreach (RuneDecalController decal in runeDecals)
        {
            if (decal != null)
            {
                Coroutine flashCoroutine = StartCoroutine(
                    decal.FlashAndFadeEmission(overallFlashPeakEmission, overallFlashDuration, overallFlashColor)
                );
                flashCoroutines.Add(flashCoroutine); // 이 단계의 코루틴만 리스트에 추가
            }
        }

        // 이 단계의 모든 코루틴이 완료될 때까지 대기
        foreach (Coroutine coroutine in flashCoroutines)
        {
            if (coroutine != null) yield return coroutine;
        }
        _runeEffectCoroutines.Clear(); // 완료된 코루틴 목록 비움
    }

    private IEnumerator Stage3_SequentialFlashAndFade()
    {
        StopAllRuneEffectCoroutines(); // 새 단계 시작 시 기존 코루틴 목록 초기화
        List<Coroutine> sequentialFlashCoroutines = new List<Coroutine>(); // 이 단계의 코루틴만 추적
        for (int i = 0; i < runeDecals.Length; i++)
        {
            RuneDecalController decal = runeDecals[i];
            if (decal != null)
            {
                // decal.enabled = true;
                Coroutine flashCoroutine = StartCoroutine(
                    decal.FlashAndFadeEmission(sequentialFlashPeakEmission, sequentialFlashDuration, individualEmissionColor)
                );
                sequentialFlashCoroutines.Add(flashCoroutine); // 이 단계의 코루틴만 리스트에 추가
                yield return new WaitForSeconds(sequentialFlashDelay);
            }
        }

        // 이 단계의 모든 코루틴이 완료될 때까지 대기
        foreach (Coroutine coroutine in sequentialFlashCoroutines)
        {
            if (coroutine != null) yield return coroutine;
        }
        _runeEffectCoroutines.Clear(); // 완료된 코루틴 목록 비움
    }

    private IEnumerator ResetRunes()
    {
        Debug.Log("룬 시퀀스 리셋 시작 (중간 이탈).");
        OnResetStart?.Invoke();

        // 현재 실행 중인 모든 룬 효과 코루틴 중지
        StopAllRuneEffectCoroutines();

        List<Coroutine> resetCoroutines = new List<Coroutine>();
        foreach (RuneDecalController decal in runeDecals)
        {
            if (decal != null)
            {
                // 이미션 페이드 아웃 코루틴 시작 및 추적
                Coroutine emissionFade = StartCoroutine(decal.FadeEmission(0f, decal.GetComponent<DecalProjector>().material.GetColor("_EmissionColor")));
                resetCoroutines.Add(emissionFade);

                // 알파 페이드 아웃 코루틴 시작 및 추적
                Coroutine alphaFade = StartCoroutine(FadeOutAlpha(decal, decal.InitialAlpha, resetFadeOutDuration));
                resetCoroutines.Add(alphaFade);
            }
        }

        // 모든 리셋 코루틴이 완료될 때까지 대기
        foreach (Coroutine coroutine in resetCoroutines)
        {
            if (coroutine != null) yield return coroutine;
        }

        // 리셋 코루틴이 모두 완료된 후에 DecalController를 비활성화
        foreach (RuneDecalController decal in runeDecals)
        {
            if (decal != null)
            {
                decal.enabled = false;
            }
        }

        Debug.Log("룬 시퀀스 리셋 완료. 룬은 서서히 사라집니다.");
        _sequenceStarted = false; // 리셋 완료 후 시퀀스 시작 상태 초기화
    }
    private IEnumerator FadeOutAlpha(RuneDecalController decal, float targetAlpha, float duration) 
    {
        // Null체크
        if (decal == null || decal.GetComponent<DecalProjector>() == null || decal.GetComponent<DecalProjector>().material == null) yield break;

        float startAlpha = decal.GetComponent<DecalProjector>().material.GetColor("_BaseColor").a;
        float timer = 0f;

        while (timer < 1f)
        {
            timer += Time.deltaTime / duration;
            float currentAlpha = Mathf.Lerp(startAlpha, targetAlpha, timer); 
            decal.SetMaterialAlpha(currentAlpha);
            yield return null;
        }
        decal.SetMaterialAlpha(targetAlpha); // 최종적으로 targetAlpha 보장
    }


    //에디터 확인용 기즈모
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, triggerDistance);
    }

    void OnValidate()
    {
        // individualRevealDelays 배열 크기 자동 조절
        if (runeDecals != null && (individualRevealDelays == null || individualRevealDelays.Length != runeDecals.Length))
        {
            float[] newDelays = new float[runeDecals.Length];
            for (int i = 0; i < runeDecals.Length; i++)
            {
                if (individualRevealDelays != null && i < individualRevealDelays.Length)
                {
                    newDelays[i] = individualRevealDelays[i];
                }
                else
                {
                    newDelays[i] = i * 0.2f; // 새로운 항목의 기본값
                }
            }
            individualRevealDelays = newDelays;
        }
    }
}