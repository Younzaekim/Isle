using UnityEngine;

public class PlayerEffectController : MonoBehaviour
{
    [SerializeField] private Transform Player;
    private Rigidbody playerRigidbody;

    [Header("이펙트 오브젝트")]
    [SerializeField] private ParticleSystem trailingParticles;
    [SerializeField] private TrailRenderer[] wingTrails;

    [Header("속도에 따른 방출 제어")]
    [SerializeField] private float minSpeedToEmit = 1.0f;
    [SerializeField] private float maxSpeedForFullEffect = 15.0f;
    [SerializeField] private float maxParticleEmissionRate = 50f;

    [Header("파티클 속성")]
    [SerializeField] private Color particleColor = Color.white;
    [SerializeField] private float particleStartSize = 0.05f;

    [Header("트레일 속성")]
    [SerializeField] private Gradient trailColorGradient;
    [SerializeField] private float trailWidthMultiplier = 1.0f;

    [Header("비행 상태 추론")]
    [Tooltip("지상으로 간주할 최대 거리를 설정")]
    [SerializeField] private float groundCheckDistance = 0.5f;
    [Tooltip("비행으로 간주하기 위해 공중에 머물러야 하는 최소 시간")]
    [SerializeField] private float timeInAirToFly = 0.3f;
    [Tooltip("Ground 설정")]
    [SerializeField] private LayerMask groundLayer;
    [Tooltip("이 값보다 빠르게 아래로 '가속'하면 자유낙하")]
    [SerializeField] private float maxFallAcceleration = -5.0f;

    private Vector3 lastPosition;
    private float currentSpeed;
    private ParticleSystem.EmissionModule particleEmissionModule;

    private bool isGrounded = true;
    private float timeInAir = 0f;

    private Vector3 lastVelocity;
    private Vector3 currentAcceleration;

    void Start()
    {
        if (Player == null)
        {
            GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
            if (playerObject != null)
            {
                Player = playerObject.transform;
            }
        }

        if (Player == null)
        {
            Debug.LogError("Player 오브젝트를 찾을 수 없음", this.gameObject);
            this.enabled = false;
            return;
        }

        playerRigidbody = Player.GetComponent<Rigidbody>();
        if (playerRigidbody == null)
        {
            Debug.LogError("Player 오브젝트에 Rigidbody 컴포넌트가 없음!", this.gameObject);
            this.enabled = false;
            return;
        }

        ApplyInitialEffectProperties();

        lastPosition = Player.position;
        // ✨ lastVelocity 초기화
        lastVelocity = playerRigidbody.linearVelocity;

        if (trailingParticles != null)
        {
            particleEmissionModule = trailingParticles.emission;
            particleEmissionModule.rateOverTime = 0;
        }

        foreach (var trail in wingTrails)
        {
            if (trail != null)
            {
                trail.emitting = false;
            }
        }
    }

    void ApplyInitialEffectProperties()
    {
        if (trailingParticles != null)
        {
            var mainModule = trailingParticles.main;
            mainModule.startColor = particleColor;
            mainModule.startSize = particleStartSize;
        }
        foreach (var trail in wingTrails)
        {
            if (trail != null)
            {
                trail.colorGradient = trailColorGradient;
                trail.widthMultiplier = trailWidthMultiplier;
            }
        }
    }

    void Update()
    {
        if (Player == null || playerRigidbody == null) return;

        CheckIfGrounded();
        CalculatePhysicsState();
        HandleEffects();
    }

    void CheckIfGrounded()
    {
        if (Physics.Raycast(Player.position, Vector3.down, groundCheckDistance, groundLayer))
        {
            isGrounded = true;
            timeInAir = 0f;
        }
        else
        {
            isGrounded = false;
            timeInAir += Time.deltaTime;
        }
    }

    void CalculatePhysicsState()
    {
        currentSpeed = (Player.position - lastPosition).magnitude / Time.deltaTime;
        lastPosition = Player.position;

        currentAcceleration = (playerRigidbody.linearVelocity - lastVelocity) / Time.deltaTime;
        lastVelocity = playerRigidbody.linearVelocity;
    }

    void HandleEffects()
    {
        bool isControlledFlight = currentAcceleration.y >= maxFallAcceleration;
        bool isLikelyFlying = !isGrounded && isControlledFlight && timeInAir >= timeInAirToFly;
        bool shouldEmit = isLikelyFlying && currentSpeed > minSpeedToEmit;

        foreach (var trail in wingTrails)
        {
            if (trail != null && trail.emitting != shouldEmit)
            {
                trail.emitting = shouldEmit;
            }
        }

        if (trailingParticles != null)
        {
            float speedRatio = shouldEmit ? Mathf.InverseLerp(minSpeedToEmit, maxSpeedForFullEffect, currentSpeed) : 0f;
            float currentEmissionRate = Mathf.Lerp(0, maxParticleEmissionRate, speedRatio);
            particleEmissionModule.rateOverTime = currentEmissionRate;
        }
    }
}