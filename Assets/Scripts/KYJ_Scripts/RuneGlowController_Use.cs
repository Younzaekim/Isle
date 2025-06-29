using UnityEngine;
using System.Collections;

[RequireComponent(typeof(LODGroup))]
public class RuneGlowController_Use_CustomShader : MonoBehaviour
{
    [Header("발광 설정")]
    [SerializeField] private float glowDistance = 5f;
    [SerializeField] private float fadeSpeed = 2f;
    [SerializeField] private Color emissionColor = Color.cyan;
    [Tooltip("true: 한번 활성화되면 계속 유지, false: 거리에 따라 ON/OFF")]
    [SerializeField] private bool persistentGlow = false;

    private Transform player;
    private MeshRenderer[] meshRenderers;
    private Material[] materials;
    private float currentIntensity = 0f;
    private float targetIntensity = 0f;
    private bool hasBeenActivated = false; // 한번이라도 활성화된 적이 있는지 체크

    void Start()
    {
        InitializePlayer();
        InitializeMaterials();
    }

    private void InitializePlayer()
    {
        GameObject existingPlayer = GameObject.FindGameObjectWithTag("Player");
        if (existingPlayer != null)
        {
            player = existingPlayer.transform;
            Debug.Log("씬에서 플레이어를 찾았습니다.");
            return;
        }

        Debug.LogWarning("플레이어를 찾지 못했습니다. 5초마다 재시도합니다.");
        StartCoroutine(TryFindPlayer());
    }

    private void InitializeMaterials()
    {
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

    private IEnumerator TryFindPlayer()
    {
        while (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                player = playerObj.transform;
                Debug.Log("플레이어를 찾았습니다!");
                yield break;
            }
            Debug.Log("플레이어를 찾지 못했습니다. 5초 후 다시 시도합니다.");
            yield return new WaitForSeconds(5f);
        }
    }

    void Update()
    {
        if (player == null) return;

        float dist = Vector3.Distance(transform.position, player.position);
        bool isInRange = dist < glowDistance;

        if (persistentGlow)
        {
            // 영구 발광 모드: 한번 활성화되면 계속 유지
            if (isInRange) hasBeenActivated = true;
            targetIntensity = hasBeenActivated ? 3f : 0f;
        }
        else
        {
            // 일시 발광 모드: 거리에 따라 ON/OFF
            targetIntensity = isInRange ? 3f : 0f;
        }

        currentIntensity = Mathf.MoveTowards(currentIntensity, targetIntensity, Time.deltaTime * fadeSpeed);

        foreach (var mat in materials)
        {
            mat.SetFloat("_DecalEmissionIntensity", currentIntensity);
        }
    }

    // 발광 상태 수동 초기화를 위한 public 메서드 추가
    public void ResetGlowState()
    {
        hasBeenActivated = false;
        targetIntensity = 0f;
        currentIntensity = 0f;
        
        if (materials != null)
        {
            foreach (var mat in materials)
            {
                mat.SetFloat("_DecalEmissionIntensity", 0f);
            }
        }
    }
}
