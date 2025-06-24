using UnityEngine;

public class SurfaceSensor : MonoBehaviour
{
    public float rayDistance = 1.2f;

    public SurfaceType CurrentSurface { get; private set; } = SurfaceType.Leave;

    private void Update()
    {
        DetectSurface();
    }

    private void DetectSurface()
    {
        Ray ray = new Ray(transform.position + Vector3.up * 0.1f, Vector3.down);
        if (Physics.Raycast(ray, out RaycastHit hit, rayDistance))
        {
            string tag = hit.collider.tag;

            switch (tag)
            {
                case "Rock": CurrentSurface = SurfaceType.Rock; break;
                case "Sand": CurrentSurface = SurfaceType.Sand; break;
                case "Wood": CurrentSurface = SurfaceType.Wood; break;
                case "Water": CurrentSurface = SurfaceType.Water; break;
                default: CurrentSurface = SurfaceType.Leave; break;
            }
        }
        else
        {
            CurrentSurface = SurfaceType.Leave;
        }
    }
}
