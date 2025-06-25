using UnityEngine;

public class ShieldFadeTrigger_Test : MonoBehaviour
{

    public float fadeDuration = 3f;
    public float finalScale = 5f;
    public Material shieldMat;

    private float fadeTime = 0f;
    private bool triggered = false;

    private void OnEnable()
    {
        StartCoroutine(FadeAndScale());
    }
    void Update()
    {
        // if (triggered) return;

        // float dist = Vector3.Distance(player.position, transform.position);
        // if (dist < activationDistance)
        // {
        //     triggered = true;
        //     StartCoroutine(FadeAndScale());
        // }
    }

    System.Collections.IEnumerator FadeAndScale()
    {
        float time = 0f;
        Vector3 startScale = Vector3.zero;
        Vector3 endScale = new Vector3(finalScale, finalScale, finalScale);
        transform.localScale = startScale;

        while (time < fadeDuration)
        {
            time += Time.deltaTime;
            float t = time / fadeDuration;

            transform.localScale = Vector3.Lerp(startScale, endScale, t);

            shieldMat.SetFloat("_FadeTime", t);

            yield return null;
        }
        shieldMat.SetFloat("_FadeTime", 1.0f);
        Destroy(gameObject, 0.2f);
    }
}
