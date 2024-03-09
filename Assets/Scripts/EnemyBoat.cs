using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyBoat : MonoBehaviour
{

    private bool targetInSightRange;
    private bool targetInShootingRange;
    private bool haveLineOfSight;

    public TrajectoryMaffs maffs;

    public Transform target;

    public NavMeshAgent navMeshAgent;
    public Animator animator;

    [SerializeField]
    private float sightDistance;
    [SerializeField]
    private float shootingDistance;

    [SerializeField]
    private GameObject projectile;


    [SerializeField]
    private float randomPathDistance; // The Radius of the circle that we will pick a point from

    private float LastAttackTime;
    
    [SerializeField]
    private float AttackDelay = 5f;

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
        LastAttackTime = Random.Range(0, 5);

    }

    private void Update()
    {
        Vector3 raycastOrigin = transform.position + new Vector3(0, 4, 0);
        
        targetInSightRange = Vector3.Distance(transform.position, target.position) <= sightDistance;

        if (targetInSightRange)
        {
            RaycastHit hit;
            if (Physics.Raycast(raycastOrigin, target.position - raycastOrigin, out hit))
            {
                // probs wanna compare using the type of collider? idk
                //Debug.Log(hit.collider.gameObject.name);
                //if (hit.collider.gameObject.name == "Player")

                if (hit.collider.gameObject.name == "Player")
                {
                    if (!haveLineOfSight)
                    {
                        haveLineOfSight = true;

                    }
                }else if (haveLineOfSight & hit.collider.gameObject.name != "Player")
                {
                    haveLineOfSight = false; 
                }

                
            }
            if (haveLineOfSight) 
            { 
                Debug.DrawRay(raycastOrigin, target.position - raycastOrigin, Color.green); 
            } else Debug.DrawRay(raycastOrigin, target.position - raycastOrigin, Color.red);

            targetInShootingRange = Vector3.Distance(transform.position, target.position) <= shootingDistance;

        } else Debug.DrawRay(raycastOrigin, target.position - raycastOrigin, Color.white);


        switch (state)
        {
            default:
            case State.Roaming:
                if (!navMeshAgent.hasPath) // if we don't have a current path, set a new one!
                {
                    UpdatePath(RandomNavmeshLocation(randomPathDistance));
                }
                break;

            case State.Chase:
                UpdatePath(target.position);
                break;

            case State.Fire:
                //get maffs
                if (target.position != transform.position & Time.time > LastAttackTime + AttackDelay) {
                    TrajectoryMaffs.ThrowData data = maffs.CalculateThrowData(target.position, transform.position);
                    //Debug.Log(data.ThrowVelocity);
                    Fire(data);
                    LastAttackTime = Time.time;
                }
                break;
        }
        // if searching AND in view range AND can see them:    start chasing
        //if (state == State.Roaming & targetInSightRange & haveLineOfSight)
        if (state == State.Roaming & targetInSightRange)
        {
            state = State.Chase;
        }

        // if target gets out of sight, go back to searching

        // TODO: if target goes around corner breaking LOS,
        //       go to last seen coordinate
        //       try to reestablish sight.
        //       if fail, go back to searching

        // if target gets out of chase range, resume roaming
        if (state == State.Chase & !targetInSightRange)
        {
            state = State.Roaming;
        }

        // should always be chasing to close distance before firing
        if (state == State.Chase & targetInShootingRange & haveLineOfSight)
        {
            state = State.Fire;

        }
        if (state == State.Fire & !targetInShootingRange & haveLineOfSight)
        {
            state = State.Chase;
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

    private void Fire(TrajectoryMaffs.ThrowData data)
    {
        Rigidbody projectile_rb = Instantiate(projectile, transform.position, transform.rotation).GetComponent<Rigidbody>();

        projectile_rb.AddForce(data.ThrowVelocity, ForceMode.Impulse);

        //SimulateTrajectory();
    }
}
