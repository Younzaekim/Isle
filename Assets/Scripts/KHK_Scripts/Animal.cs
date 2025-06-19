using UnityEngine;
using UnityEngine.AI;

public class Animal : MonoBehaviour
{
    protected NavMeshAgent agent;
    protected Rigidbody rb;

    protected void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        rb = GetComponent<Rigidbody>();

    }  
}