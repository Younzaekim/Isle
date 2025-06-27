using System.Collections.Generic;
using UnityEngine;

public class PlayerPathGizmos : MonoBehaviour
{
    private List<Vector3> positions = new List<Vector3>();
    private int maxPositions = 100;

    private void Update()
    {
        Vector3 currentPos = transform.position;

        if (positions.Count == 0 || Vector3.Distance(positions[positions.Count - 1], currentPos) > 0.1f)
        {
            positions.Add(currentPos);

            if (positions.Count > maxPositions)
                positions.RemoveAt(0);
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;

        for (int i = 1; i < positions.Count; i++)
        {
            Gizmos.DrawLine(positions[i - 1], positions[i]);
        }
    }
}
