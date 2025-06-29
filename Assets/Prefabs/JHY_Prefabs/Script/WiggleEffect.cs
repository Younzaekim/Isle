using UnityEngine;

public class WiggleEffect : MonoBehaviour
{
    [Header("Èçµé¸² ¼³Á¤")]
    [Tooltip("Èçµé¸²ÀÇ °­µµ")]
    public float noiseStrength = 0.1f;

    [Tooltip("Èçµé¸²ÀÇ ºóµµ")]
    public float noiseFrequency = 1.0f;

    private Vector3 originalLocalPosition;
    private float timeOffset; 

    void Start()
    {
        originalLocalPosition = transform.localPosition;
        timeOffset = Random.Range(0f, 100f);
    }

    void Update()
    {
        float xNoise = (Mathf.PerlinNoise(Time.time * noiseFrequency + timeOffset, 0) - 0.5f) * 2f;
        float yNoise = (Mathf.PerlinNoise(0, Time.time * noiseFrequency + timeOffset) - 0.5f) * 2f;

        Vector3 noiseOffset = new Vector3(xNoise, yNoise, 0) * noiseStrength;
        transform.localPosition = originalLocalPosition + noiseOffset;
    }
}