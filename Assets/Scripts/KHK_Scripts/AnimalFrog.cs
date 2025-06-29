using UnityEngine;
using UnityEngine.AI;

public class AnimalFrog : Animal
{
    [SerializeField] private float detectRange = 2f;      //감지 반경

    [SerializeField] private float moveInterval = 2f;     //이동 하는 텀

    [SerializeField] private Transform[] targets;
    [SerializeField] private LayerMask whatIsPlayer;

    private float nextMoveTime;
    private bool isReached;
   
    private int targetIndex = -1;

    protected override void Start()
    {
        base.Start();
        nextMoveTime = Time.time + moveInterval;
    }

    void Update()
    {       

        AnimState();
        if (isInteracted == false) // 상호작용되지 않았을때만
        {
            Collider[] hits = Physics.OverlapSphere(transform.position, detectRange, whatIsPlayer);

            foreach (var hit in hits)
            {
                if (hit.gameObject == player)
                {
                    isDetected = true;
                    break;
                }
            }

            if (isDetected)
            {
                if (Time.time >= nextMoveTime)
                {
                    SetNextDestination();
                    if (isReached)
                    {
                        nextMoveTime = Time.time + moveInterval;
                        isReached = false;
                    }
                }
            }
        }
        else
        {
            float offset = 1.5f;
            Vector3 playerBack = player.transform.position - player.transform.forward * offset;
            agent.SetDestination(playerBack);

            Vector3 lookDir = (player.transform.position - transform.position).normalized;

            transform.rotation = Quaternion.LookRotation(lookDir) * Quaternion.Euler(0, -90, 0);
        }
    }

    private void AnimState()
    {
        anim.SetBool("Move", agent.isOnOffMeshLink);
    }

    private void SetNextDestination()
    {
        if (agent.pathStatus == NavMeshPathStatus.PathComplete && agent.remainingDistance < 0.1f)
        {
            isReached = true;

            targetIndex += 1;

            if(targetIndex >= targets.Length)
            {
                targetIndex = 0;
            }

            Vector3 targetPosition = targets[targetIndex].position;

            Vector3 direction = (targetPosition - transform.position).normalized;

            direction.y = 0;

            transform.rotation = Quaternion.LookRotation(direction);

            agent.SetDestination(targets[targetIndex].position);

            Vector3 lookDir = (targets[targetIndex].position - transform.position).normalized;
            transform.rotation = Quaternion.LookRotation(lookDir) * Quaternion.Euler(0, -90, 0);
        }
    }
    
}