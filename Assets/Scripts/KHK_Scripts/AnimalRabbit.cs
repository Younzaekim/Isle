using UnityEngine;
using UnityEngine.AI;

public class AnimalRabbit : Animal
{
    [SerializeField] private float detectRange = 2f;      // ���� �ݰ�
    [SerializeField] private float escapeDistance = 5f;   // ���� �Ÿ�
    [SerializeField] private float moveInterval = 3f;     // �̵��ϴ� ��
    [SerializeField] private LayerMask whatIsPlayer;

    private float nextMoveTime;
    private bool isReached;
    private float nextDetectTime;
    private float detectInterval = 0.5f; // ���� �ֱ�(��)

    protected override void Start()
    {
        base.Start();
        nextMoveTime = Time.time + moveInterval;
        isReached = true;
        agent.updateRotation  = false;
    }

    void Update()
    {
        AnimState();

        if (isInteracted == false)
        {
            if (Time.time >= nextDetectTime)
            {
                nextDetectTime = Time.time + detectInterval;
                isDetected = false;
                Collider[] hits = Physics.OverlapSphere(transform.position, detectRange, whatIsPlayer);
                foreach (var hit in hits)
                {
                    Debug.Log("Detecting Player");
                    if (hit.gameObject.CompareTag("Player"))
                    {
                        isDetected = true;
                        Vector3 runDir = (transform.position - player.transform.position).normalized;
                        Vector3 runPos = transform.position + runDir * escapeDistance;

                        agent.SetDestination(runPos);

                        Vector3 lookDir = (runPos - transform.position).normalized;
                        transform.rotation = Quaternion.LookRotation(lookDir) * Quaternion.Euler(0, -90, 0);
                        isReached = false;
                        break;
                    }
                }
            }

            if (!isDetected)
            {
                if (isReached && Time.time >= nextMoveTime)
                {
                    SetRandomDestination();
                    nextMoveTime = Time.time + moveInterval;
                }
            }

            if (!isReached && agent.pathStatus == NavMeshPathStatus.PathComplete && agent.remainingDistance < 0.1f)
            {
                isReached = true;
            }
        }
        else if (isInteracted == true)// ��ȣ�ۿ���� ��
        {
            isDetected = false;
            float offset = 1.5f;
            Vector3 playerBack = player.transform.position - player.transform.forward * offset;
            agent.SetDestination(playerBack);

            Vector3 lookDir = (player.transform.position - transform.position).normalized;
            Quaternion targetRot = Quaternion.LookRotation(lookDir);
            transform.rotation = targetRot * Quaternion.Euler(0, -90, 0);

        }
    }

    private void AnimState()
    {
        anim.SetBool("Move", agent.velocity.magnitude >= 0.01f);
    }

    private void SetRandomDestination()
    {
        Vector3 randomDirection = Random.insideUnitSphere * 5f;
        randomDirection += transform.position;
        NavMeshHit navHit;
        if (NavMesh.SamplePosition(randomDirection, out navHit, detectRange, NavMesh.AllAreas))
        {
            agent.SetDestination(navHit.position);
            isReached = false;

            Vector3 lookDir = (navHit.position - transform.position).normalized;
            transform.rotation = Quaternion.LookRotation(lookDir) * Quaternion.Euler(0, -90, 0);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.white;
        Gizmos.DrawWireSphere(transform.position, detectRange);
    }
}