using UnityEngine;

public class EmissionController : MonoBehaviour
{
    public Transform player;
    public Material mat;

    public float maxDistance = 5f;
    public float maxEmissionIntensity = 5f;
    public Color baseEmissionColor = Color.red;

    void Update()
    {
        float dist = Vector3.Distance(player.position, transform.position);
        float t = Mathf.InverseLerp(maxDistance, 0f, dist);
        float intensity = Mathf.Lerp(0f, maxEmissionIntensity, t);

        Color emissive = baseEmissionColor * intensity;

        mat.SetColor("_EmissionColor", emissive);
    }
}
