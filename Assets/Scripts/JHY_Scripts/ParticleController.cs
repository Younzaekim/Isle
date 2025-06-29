using UnityEngine;

[RequireComponent(typeof(ParticleSystem))]
public class EngineParticleController : MonoBehaviour
{
    public Rigidbody playerRigidbody;
    [Header("파티클 등장 속도")]
    [Tooltip("이 속도 이상이면 파티클 출력 시작")]
    public float speedThreshold = 1f; // 이 속도 이상이면 파티클 출력 시작
    [Header("최대 파티클 양")]
    [Tooltip("최대 속도일 때 최대 파티클 출력")]
    public float maxSpeed = 10f;      // 최대 속도일 때 최대 파티클 출력
    public float maxEmissionRate = 50f;

    private ParticleSystem ps;
    private ParticleSystem.EmissionModule emission;

    void Start()
    {
        if (playerRigidbody == null)
        {
            GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
            if (playerObject != null)
            {
                playerRigidbody = playerObject.GetComponent<Rigidbody>();
                Debug.Log("RuneSequenceManager:'Player'태그로 찾음" + playerRigidbody.name);
            }
            else
            {
                Debug.LogError("RuneSequenceManager:'Player'태그로 못 찾음", this);
                enabled = false;
                return;
            }
        }

        ps = GetComponent<ParticleSystem>();
        emission = ps.emission;
    }

    void Update()
    {
        float speed = playerRigidbody.linearVelocity.magnitude;

        if (speed < speedThreshold)
        {
            emission.rateOverTime = 0f;
            return;
        }

        float t = Mathf.InverseLerp(speedThreshold, maxSpeed, speed); 
        emission.rateOverTime = t * maxEmissionRate;
    }
}
