using UnityEngine;

[RequireComponent(typeof(LODGroup))]
public class RuneGlowController_CustomShader : MonoBehaviour
{
    public float glowDistance = 5f;
    public float fadeSpeed = 2f;
    public Color emissionColor = Color.cyan;

    private Transform player;
    private MeshRenderer[] meshRenderers;
    private Material[] materials;
    private float currentIntensity = 0f;
    private float targetIntensity = 0f;

    void Start()
    {
        GameObject playerObj = GameObject.Find("Player");
        if (playerObj != null)
            player = playerObj.transform;
        else
            Debug.LogError("플레이어 없음");

        meshRenderers = GetComponentsInChildren<MeshRenderer>();
        materials = new Material[meshRenderers.Length];

        for (int i = 0; i < meshRenderers.Length; i++)
        {
            materials[i] = meshRenderers[i].material;

            materials[i].SetFloat("_DecalEmissionIntensity", 0f);
            materials[i].SetColor("_DecalEmissionColor", emissionColor);
            materials[i].SetFloat("_DecalEmissionToggle", 1f);
        }
    }

    void Update()
    {
        if (player == null) return;

        float dist = Vector3.Distance(transform.position, player.position);
        targetIntensity = (dist < glowDistance) ? 3f : 0f; 
        currentIntensity = Mathf.MoveTowards(currentIntensity, targetIntensity, Time.deltaTime * fadeSpeed);

        foreach (var mat in materials)
        {
            mat.SetFloat("_DecalEmissionIntensity", currentIntensity);
        }
    }

}
