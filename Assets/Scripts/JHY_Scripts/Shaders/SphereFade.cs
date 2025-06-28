using UnityEngine;

public class SphereFade : MonoBehaviour
{
    private Material mat;
    private float alpha = 1f;
    public float fadeSpeed = 1f;

    void Start()
    {
        Renderer renderer = GetComponent<Renderer>();
        mat = renderer.material; // 이게 반드시 인스턴스
        Debug.Log("Material Name: " + mat.name);
    }

    void Update()
    {
        if (alpha > 0f)
        {
            alpha -= Time.deltaTime * fadeSpeed;
            alpha = Mathf.Clamp01(alpha);

            Color color = mat.GetColor("_BaseColor");
            color.a = alpha;
            mat.SetColor("_BaseColor", color);

            Debug.Log("Alpha: " + alpha);
        }
    }
}
