using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyBoat : MonoBehaviour
{   

    private bool targetInSightRange;
    private bool targetInShootingRange;


    public Transform target;

    public NavMeshAgent navMeshAgent;
    public Animator animator;

    [SerializeField]
    private float sightDistance;
    [SerializeField]
    private float shootingDistance;
    

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
        targetInSightRange = Vector3.Distance(transform.position, target.position) <= sightDistance;
        targetInShootingRange = Vector3.Distance(transform.position, target.position) <= shootingDistance;

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
         // if searching and see target, start chasing
        if (targetInSightRange & state == State.Roaming)
        {
        state = State.Chase;
        }

        // if target gets out of sight, go back to searching
        if (!targetInSightRange & state == State.Chase)
        {
            state = State.Roaming;
        }

        // should always be chasing to close distance before firing
        if (targetInShootingRange & state == State.Chase)
        {
            state = State.Fire;
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
