using UnityEngine;

public class EmissionBlink : MonoBehaviour
{
    public float blinkIntensity = 5f;      
    public float transitionDuration = 0.5f; 

    private Material mat;
    private Color originalEmissionColor;
    private Color targetBlinkColor;
    private bool isBlinking = false;

    void Start()
    {
        mat = GetComponent<Renderer>().material;
        originalEmissionColor = mat.GetColor("_EmissionColor");

        targetBlinkColor = Color.red * blinkIntensity;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E) && !isBlinking)
        {
            Debug.Log("EÅ° ´©¸§");
            StartCoroutine(SmoothBlink());
        }
    }

    System.Collections.IEnumerator SmoothBlink()
    {
        isBlinking = true;

        float time = 0f;

        while (time < transitionDuration)
        {
            time += Time.deltaTime;
            float t = time / transitionDuration;

            Color currentColor = Color.Lerp(originalEmissionColor, targetBlinkColor, t);
            mat.SetColor("_EmissionColor", currentColor);

            yield return null;
        }

        time = 0f;

        while (time < transitionDuration)
        {
            time += Time.deltaTime;
            float t = time / transitionDuration;

            Color currentColor = Color.Lerp(targetBlinkColor, originalEmissionColor, t);
            mat.SetColor("_EmissionColor", currentColor);

            yield return null;
        }

        isBlinking = false;
    }
}
