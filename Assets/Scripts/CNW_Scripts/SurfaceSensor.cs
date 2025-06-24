using UnityEngine;

public class SurfaceSensor : MonoBehaviour
{
    public LayerMask surfaceMask;
    public float rayDistance = 1.2f;

    public SurfaceType CurrentSurface { get; private set; } = SurfaceType.Leave;

    private void Update()
    {
        DetectSurface();
    }

    private void DetectSurface()
    {
        Ray ray = new Ray(transform.position + Vector3.up * 0.1f, Vector3.down);
        if (Physics.Raycast(ray, out RaycastHit hit, rayDistance, surfaceMask))
        {
            int layer = hit.collider.gameObject.layer;

            switch (layer)
            {
                case 6: CurrentSurface = SurfaceType.Rock; break;
                case 7: CurrentSurface = SurfaceType.Sand; break;
                case 8: CurrentSurface = SurfaceType.Wood; break;
                case 9: CurrentSurface = SurfaceType.Water; break;
                default: CurrentSurface = SurfaceType.Leave; break;
            }
        }
        else
        {
            CurrentSurface = SurfaceType.Leave;
        }
    }
}
