using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Composites;

public class CannonSteer : MonoBehaviour
{

    [SerializeField]
    float speed = 0.5f;
    [SerializeField]
    float turnRate = 0.7f;

    [SerializeField]
    float minPower ;
    [SerializeField]
    float maxPower;

    private Vector2 steerInput;
    private Vector2 cannonPowerInput;

    public GameObject cannon_tube;
    public GameObject cannon_base;
    public GameObject projectile;

    // Line Render

    [SerializeField]
    public LineRenderer LineRenderer;
    [SerializeField]
    private Transform FirePoint;

    /*[SerializeField]
    [Range(1, 100)]*/
    private float launchForce;

    [SerializeField]
    [Range(1, 10)]
    private float ExplosionDelay = 5f;

    //[SerializeField]
    //private GameObject ExplosionParticleSystem;

    [Header("Line Render controls UwU")]
    [SerializeField]
    [Range(10, 100)]
    private int LinePoints = 175;

    [SerializeField]
    [Range(0.01f, 0.25f)]
    private float timeIntervalInPoints = 0.1f;

    [SerializeField]
    private LayerMask ProjectileCollisionMask;



    // ----- maffs stuff
    [Range(0, 1)]
    [Tooltip("Using a values closer to 0 will make the agent throw with the lower force"
       + "down to the least possible force (highest angle) to reach the target.\n"
       + "Using a value of 1 the agent will always throw with the MaxThrowForce below.")]
    public float ForceRatio = 0;
    [SerializeField]
    [Tooltip("If the required force to throw the attack is greater than this value, "
        + "the agent will move closer until they come within range.")]
    private float MaxThrowForce = 25;



    void Start()
    {
        launchForce = minPower;
    }

    // Update is called once per frame
    void Update()
    {
        //float turnAngle = steerInput.x * turnRate;
        //float fireAngle = steerInput.y * turnRate;

        launchForce = launchForce + cannonPowerInput.y;

        //cannon_base.transform.Rotate(Vector3.up, turnAngle); //Turn the ship based on the horizontal input received
        //cannon_tube.transform.Rotate(Vector3.left, fireAngle);

        SimulateTrajectory();
    }

    /*void OnCannonMove(InputValue value)
    {
        steerInput = value.Get<Vector2>(); //Store the new vector any time the move vector changes
    }*/

    void OnCannonPower(InputValue value)
    {
        
        launchForce += value.Get<float>();
        //launchForce = Mathf.Clamp(launchForce, minPower, maxPower);
        
    }

    void OnFire()
    {
        // init a new cannonball
        GameObject projectile_copy = Instantiate(projectile, FirePoint.transform.position, FirePoint.transform.rotation);

        //apply force
        Rigidbody projectile_rb = projectile_copy.GetComponent<Rigidbody>();

        projectile_rb.AddForce(FirePoint.transform.forward * launchForce, ForceMode.Impulse);

        SimulateTrajectory();
    }


    private ThrowData CalculateThrowData(Vector3 TargetPosition, Vector3 StartPosition)
    {
        // v = initial velocity, assume max speed for now
        // x = distance to travel on X/Z plane only
        // y = difference in altitudes from thrown point to target hit point
        // g = gravity

        Vector3 displacement = new Vector3(
            TargetPosition.x,
            StartPosition.y,
            TargetPosition.z
        ) - StartPosition;
        float deltaY = TargetPosition.y - StartPosition.y; // change in elevation of targets
                                                           // think boat on water vs. bunker on mountain
        float deltaXZ = displacement.magnitude; // Distance away on ground

        // find lowest initial launch velocity with other magic formula from https://en.wikipedia.org/wiki/Projectile_motion
        // v^2 / g = y + sqrt(y^2 + x^2)
        // meaning.... v = sqrt(g * (y+ sqrt(y^2 + x^2)))
        float gravity = Mathf.Abs(Physics.gravity.y);
        float throwStrength = Mathf.Clamp(
            Mathf.Sqrt(
                gravity
                * (deltaY + Mathf.Sqrt(Mathf.Pow(deltaY, 2)
                + Mathf.Pow(deltaXZ, 2)))),
            0.01f,
            MaxThrowForce
        );
        throwStrength = Mathf.Lerp(throwStrength, MaxThrowForce, ForceRatio);

        float angle;
        if (ForceRatio == 0)
        {
            // optimal angle is chosen with a relatively simple formula
            angle = Mathf.PI / 2f - (0.5f * (Mathf.PI / 2 - (deltaY / deltaXZ)));
        }
        else
        {
            // when we know the initial velocity, we have to calculate it with this formula
            // Angle to throw = arctan((v^2 +- sqrt(v^4 - g * (g * x^2 + 2 * y * v^2)) / g*x)
            angle = Mathf.Atan(
                (Mathf.Pow(throwStrength, 2) - Mathf.Sqrt(
                    Mathf.Pow(throwStrength, 4) - gravity
                    * (gravity * Mathf.Pow(deltaXZ, 2)
                    + 2 * deltaY * Mathf.Pow(throwStrength, 2)))
                ) / (gravity * deltaXZ)
            );
        }

        if (float.IsNaN(angle))
        {
            // you will need to handle this case when there
            // is no feasible angle to throw the object and reach the target.
            return new ThrowData();
        }

        Vector3 initialVelocity =
            Mathf.Cos(angle) * throwStrength * displacement.normalized
            + Mathf.Sin(angle) * throwStrength * Vector3.up;

        return new ThrowData
        {
            ThrowVelocity = initialVelocity,
            Angle = angle,
            DeltaXZ = deltaXZ,
            DeltaY = deltaY
        };
    }

    private struct ThrowData
    {
        public Vector3 ThrowVelocity;
        public float Angle;
        public float DeltaXZ;
        public float DeltaY;
    }



    //make sure that FirePoint is set to use world space in the editor or this won't work
    private void SimulateTrajectory()
    {
        //https://github.com/llamacademy/projectile-trajectory/blob/main/Assets/Scripts/GrenadeThrower.cs

        LineRenderer.positionCount = Mathf.CeilToInt(LinePoints / timeIntervalInPoints) + 1;
        Vector3 startPosition = FirePoint.position;
        Vector3 startVelocity = launchForce * FirePoint.forward;
        int i = 0;
        LineRenderer.SetPosition(i, startPosition);
        for (float time = 0; time < LinePoints; time += timeIntervalInPoints)
        {
            i++;
            Vector3 point = startPosition + time * startVelocity;
            point.y = startPosition.y + startVelocity.y * time + (Physics.gravity.y / 2f * time * time);

            LineRenderer.SetPosition(i, point);

            // Raycast to stop trajectory calc when it hits the terrain
            Vector3 lastPosition = LineRenderer.GetPosition(i - 1);

            if (Physics.Raycast(lastPosition, (point - lastPosition).normalized, out RaycastHit hit, (point - lastPosition).magnitude, ProjectileCollisionMask))
            { //Raycast to find only the ground object and then truncate the line at the point we're at
                LineRenderer.positionCount = i;
                return;
            }

        }


        
        
    }

}
        
   


