using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;


public class EnemyBoat : MonoBehaviour
{
    [SerializeField]
    bool showAgentPath = true;

    [SerializeField]
    bool showAgentLOS = false;

    //to show in editor
    [SerializeField]
    State currentState;
    [SerializeField]
    Vector3 currentPos;
    [SerializeField]
    Vector3 goingTo;
    [SerializeField]
    int pathNumPoints;




    private int health, maxHealth = 1;

    private bool targetInSightRange;
    private bool targetInShootingRange;
    private bool haveLineOfSight;

    public TrajectoryMaffs maffs;

    public Transform target;

    public NavMeshAgent navMeshAgent;
    public Animator animator;
    public LineRenderer line;

    [SerializeField]
    private float sightDistance;
    [SerializeField]
    private float shootingDistance;

    [SerializeField]
    private GameObject projectile;

    [SerializeField]
    private LayerTerrain layerTerrain;

    [SerializeField]
    private float randomPathDistance; // The Radius of the circle that we will pick a point from

    private float LastAttackTime;
    
    [SerializeField]
    private float AttackDelay = 5f;


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
        line = GetComponent<LineRenderer>();
    }

    private void Start()
    {
        LastAttackTime = Random.Range(0, 5);
        health = maxHealth;
        state = State.Roaming;
    }

    private void Update()
    {
        currentPos = transform.position;

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

            if (haveLineOfSight & showAgentLOS) 
            {
                Debug.DrawRay(raycastOrigin, target.position - raycastOrigin, Color.green); 
            } else if (showAgentLOS) Debug.DrawRay(raycastOrigin, target.position - raycastOrigin, Color.red);

            targetInShootingRange = Vector3.Distance(transform.position, target.position) <= shootingDistance;

        } else if (showAgentLOS) Debug.DrawRay(raycastOrigin, target.position - raycastOrigin, Color.white);

        if (showAgentPath && navMeshAgent.hasPath)
        {
            line.SetPosition(0, transform.position); //set the line's origin
            var path = navMeshAgent.path;

            if (path.corners.Length < 2)
            { //if the path has 1 or no corners, there is no need
                return;                 
            }

            line.positionCount = path.corners.Length; //set the array of positions to the amount of corners
            line.SetPositions(path.corners); //go through each corner and set that to the line renderer's position
        }

        switch (state)
        {
            default:
            case State.Roaming:
                if (!navMeshAgent.hasPath && !navMeshAgent.pathPending) // if we don't have a current path, set a new one!
                {
                    Vector3 next_roaming_point = RandomNavmeshLocation(randomPathDistance);
                    UpdatePath(next_roaming_point);
                }
                break;

            case State.Chase:
                // get a point that's between the max shooting distance
                // and some arbitrary point within that distance
                float min_shootingDistance = shootingDistance / 2; //whatever man for now just halfway between the player and the outer shooting range
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
            //state = State.Fire;

        }
        if (state == State.Fire & !targetInShootingRange & haveLineOfSight)
        {
            state = State.Chase;
        }

        /*// temp not using LOS because not working rn
        if (state == State.Chase & targetInShootingRange)
        {
            //state = State.Fire;

        }
        if (state == State.Fire & !targetInShootingRange)
        {
            state = State.Chase;
        }*/

        currentState = state;
    
    } // End of Update() --------------------------------------------------------------


    public Vector3 RandomNavmeshLocation(float radius)
    {
        bool foundPosition = false;
        Vector3 randomPointInCircle = Vector3.zero;

        while (!foundPosition)
        {

            randomPointInCircle = Random.insideUnitSphere * radius; // specifying unityengine because using System temporarily
            randomPointInCircle = transform.position + randomPointInCircle;

            //make sure value is on the map, and set Y to water level
            randomPointInCircle.x = Mathf.Clamp(randomPointInCircle.x, 0, layerTerrain.X - 1);
            randomPointInCircle.y = layerTerrain.waterheight_int;
            randomPointInCircle.z = Mathf.Clamp(randomPointInCircle.z, 0, layerTerrain.Y - 1);
            try //temp in try catch
            {
                if (layerTerrain.finalMap.GetTile((int)randomPointInCircle.z, (int)randomPointInCircle.x).ValuesHere["Land"] == 0)
                {
                    foundPosition = true;
                }
            }
            catch (System.Exception e)
            {
                Debug.Log("GetTile() failed to get tile:  "+(int)randomPointInCircle.z+ "  /  " + (int)randomPointInCircle.x);
                Debug.Log(e);
            }
        }

        

        // now we run that point through the navmesh point finder to get a point ON the navmesh, and that should fix the problem?
        //NavMeshHit hit;
        Vector3 finalPosition = Vector3.zero;
        if (NavMesh.SamplePosition(randomPointInCircle, out NavMeshHit hit, 5, 1))
        {
            finalPosition += hit.position;
        } else Debug.Log("Bad Navmesh Point " + randomPointInCircle + "  (This Error Should Never Be Seen!!)");
        return finalPosition;

        //return randomPointInCircle;
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
        if (navMeshAgent.hasPath || navMeshAgent.pathPending) goingTo = destination;
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
