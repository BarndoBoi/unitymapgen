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
    public Material LineRendererMaterial;
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




    void Start()
    {
        launchForce = minPower;
        LineRenderer.material = LineRendererMaterial;

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
        
   


