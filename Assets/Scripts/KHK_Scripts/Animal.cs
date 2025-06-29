using UnityEngine;
using UnityEngine.AI;

public class Animal : MonoBehaviour
{
    protected NavMeshAgent agent;
    protected Rigidbody rb;
    protected Animator anim;
    protected GameObject player;

    public bool isDetected;
    public bool isInteracted = false;

    protected virtual void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        rb = GetComponent<Rigidbody>();
        anim = GetComponent<Animator>();
        player = GameObject.FindGameObjectWithTag("Player");
    }

    public virtual void Interact()
    {
        agent.ResetPath();

        isInteracted = true;
    }
}