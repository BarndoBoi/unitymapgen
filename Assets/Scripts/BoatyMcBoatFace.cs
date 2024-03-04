using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class BoatyMcBoatFace : MonoBehaviour
{
    public Transform target;

    public NavMeshAgent navMeshAgent;
    public Animator animator;


    [SerializeField]
    private float shootingDistance;

    [SerializeField]
    public float pathUpdateDelay = 0.2f;

    [SerializeField]
    private float pathUpdateDeadline;

    private void Awake()
    {
        navMeshAgent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
    }

    private void Start()
    {
        
        
    }

    private void Update()
    {
        if(target != null)
        {
            bool inRange = Vector3.Distance(transform.position, target.position) <= shootingDistance; 
            if (inRange)
            {
                LookAtTarget();
                
            } else { UpdatePath(); }
        }
    }

    private void LookAtTarget()
    {
        Vector3 lookPos = target.position - transform.position;
        lookPos.y = 0;
        Quaternion rotation = Quaternion.LookRotation(lookPos);
        transform.rotation = Quaternion.Slerp(transform.rotation, rotation, 0.2f);
    }

    private void UpdatePath()
    {
        if (Time.time >= pathUpdateDeadline)
        {
            Debug.Log(" updating path");
            pathUpdateDeadline = Time.time + pathUpdateDelay;
            navMeshAgent.SetDestination(target.position);
        }
    }
}
