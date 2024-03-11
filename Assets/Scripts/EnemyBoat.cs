using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyBoat : MonoBehaviour
{
    private int health, maxHealth = 1;

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
        health = maxHealth;

    }

    private void Update()
    {
        Vector3 raycastOrigin = transform.position + new Vector3(0, 5, 0);
        targetInSightRange = Vector3.Distance(transform.position, target.position) <= sightDistance;

        if (targetInSightRange)
        {
            
            if (Physics.Raycast(raycastOrigin, target.position - raycastOrigin, out RaycastHit hit))
            {
                              
                if (hit.collider.gameObject.name == "mesh")
                {
                    GameObject meshParentGameObject = hit.collider.gameObject.transform.parent.gameObject;
                    string meshParentGameObject_name = hit.collider.gameObject.transform.parent.name;

                    if (meshParentGameObject_name == "Player" || meshParentGameObject_name == "Player(Clone)")
                    {
                        if (!haveLineOfSight)
                        {
                            haveLineOfSight = true;
                        }

                    }
                } 
                else if (haveLineOfSight & hit.collider.gameObject.name == "Terrain") //just lost line of sight
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
                // get a point that's between the max shooting distance
                // and some arbitrary point within that distance
                float min_shootingDistance = shootingDistance / 2;
                float randomShootingDistanceFromTarget = Random.Range(shootingDistance, min_shootingDistance);
                Vector3 direction = (transform.position - target.position);
                Vector3 randomPointInShootingRange = target.position + Vector3.ClampMagnitude(direction,randomShootingDistanceFromTarget);

                // TODO: I still need to check if point is on water...  
                // currently I think the NavMeshAgent will, when given a land point,
                // set its destination to the furthest point on the path it CAN reach.
                // it definitely works, but it's not correct lol
                // but at least no more clusterfuck of enemies
                // trying to get inside the player xD
                UpdatePath(randomPointInShootingRange);
                break;

            case State.Fire:
                //get maffs
                if (target.position != transform.position & Time.time > LastAttackTime + AttackDelay) {
                    //TODO: this will be fixed when we have the cannons working, for now need to instantiate above boat so it doesn't instantiate inside itself
                    Vector3 boatTop = transform.position + new Vector3(0, 4, 0);
                    TrajectoryMaffs.ThrowData data = maffs.CalculateThrowData(target.position, boatTop); // need to instantiate ABOVE the boat so it doesn't hit it.
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
        //TODO: this will be fixed when we have the cannons working, for now need to instantiate above boat so it doesn't instantiate inside itself
        Vector3 boatTop = transform.position + new Vector3(0, 4, 0);
        Rigidbody projectile_rb = Instantiate(projectile, boatTop, transform.rotation).GetComponent<Rigidbody>();

        projectile_rb.AddForce(data.ThrowVelocity, ForceMode.Impulse);

        //SimulateTrajectory();
    }

    public void TakeDamage(int damage)
    {
        health -= damage;
        if (health <= 0)
        {
            Destroy(gameObject);
        }
    }
}
