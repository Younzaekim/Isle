using UnityEngine;
using UnityEngine.AI;

public class AnimalRabbit : Animal
{
    [SerializeField] private float detectRange = 3f;      // 감지 반경
    [SerializeField] private float escapeDistance = 5f;   // 도망 거리

    private Transform playerTransform;
    private bool isRunLeft = true;

    protected new void Start()
    {
        base.Start();
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
            playerTransform = playerObj.transform;
    }

    void Update()
    {
        if (playerTransform == null) return;
               
        Collider[] hits = Physics.OverlapSphere(transform.position, detectRange);
        foreach (var hit in hits)
        {
            if (hit.CompareTag("Player"))
            {
                Vector3 runDir = (transform.position - playerTransform.position).normalized;
                //여기서 runDir에 다른 벡터 더해서 최종 방향 결정시켜야함 

                Vector3 runPos = transform.position + runDir * escapeDistance;

                NavMeshHit navHit;
                if (NavMesh.SamplePosition(runPos, out navHit, escapeDistance, NavMesh.AllAreas))
                {
                    agent.SetDestination(navHit.position);
                }
                break;
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.white;
        Gizmos.DrawWireSphere(transform.position, detectRange);
    }
}