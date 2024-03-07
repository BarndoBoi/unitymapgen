using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyBoat : MonoBehaviour
{   

    private bool targetInRange;


    public Transform target;

    public NavMeshAgent navMeshAgent;
    public Animator animator;


    [SerializeField]
    private float shootingDistance; //how far from the boat before we "notice it" and try to collide with it
                                    // this will obvs be the distance where it will start shooting us in the future lol

    [SerializeField]
    private float randomPathDistance; // The Radius of the circle that we will pick a point from



    /*    [SerializeField]
        public float pathUpdateDelay = 0.2f;

        [SerializeField]
        private float pathUpdateDeadline;*/

    private enum State
    {
        Roaming,
        Chase,
        Fire,
    }
    private State state;

    private void Awake()
    {
        navMeshAgent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        state = State.Roaming;
    }

    private void Start()
    {
        
        
    }

    private void Update()
    {
        targetInRange = Vector3.Distance(transform.position, target.position) <= shootingDistance;

        switch (state)
        {
            default:
            case State.Roaming:
                if (navMeshAgent.hasPath != true) // if we don't have a current path, set a new one!
                {
                    UpdatePath(RandomNavmeshLocation(randomPathDistance));
                }
                break;

            case State.Chase:
                UpdatePath(target.position);
                break;

            case State.Fire:
                // fire at player
                break;
        }
        if (targetInRange & state == State.Roaming)
        {
        state = State.Chase;
        }

        if (!targetInRange & state == State.Chase)
        {
            state = State.Roaming;
        }
    }


    public Vector3 RandomNavmeshLocation(float radius)
    {
        Vector3 randomDirection = Random.insideUnitSphere * radius;
        randomDirection += transform.position;
        NavMeshHit hit;
        Vector3 finalPosition = Vector3.zero;
        if (NavMesh.SamplePosition(randomDirection, out hit, radius, 1))
        {
            finalPosition = hit.position;
        }
        return finalPosition;
    }

    private void LookAtTarget()
    {
        Vector3 lookPos = target.position - transform.position;
        lookPos.y = 0;
        Quaternion rotation = Quaternion.LookRotation(lookPos);
        transform.rotation = Quaternion.Slerp(transform.rotation, rotation, 0.2f);
    }

    private void UpdatePath(Vector3 destination)
    {    
    navMeshAgent.SetDestination(destination);
    }
}
