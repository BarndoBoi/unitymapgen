using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class CannonSteer : MonoBehaviour
{

    [SerializeField]
    float speed = 0.5f;
    [SerializeField]
    float turnRate = 0.7f;
    [SerializeField]
    float minimumInput = 0.01f; //Can't go slower than this

    

    private Vector2 steerInput;

    public GameObject cannon_tube;
    public GameObject cannon_base;
    public GameObject projectile;
    public Rigidbody projectile_rb;
    //public GameObject FirePoint;

    // Line Render
    
    [SerializeField]
    public LineRenderer LineRenderer;
    [SerializeField]
    private Transform FirePoint;

    [SerializeField]
    [Range(1, 100)]
    private float launchForce = 15f;

    [SerializeField]
    [Range(1, 10)]
    private float ExplosionDelay = 5f;

    //[SerializeField]
    //private GameObject ExplosionParticleSystem;

    [Header("Line Render controls UwU")]
    [SerializeField]
    [Range(10, 100)]
    private int LinePoints = 25;

    [SerializeField]
    [Range(0.01f, 0.25f)]
    private float TimeBetweenPoints = 0.1f;

 


    // Start is called before the first frame update
    void Start()
    {
        cannon_tube = GameObject.Find("cannon_tube");
        cannon_base = GameObject.Find("cannon_base");
        projectile = GameObject.Find("projectile");
        projectile_rb = projectile.GetComponent<Rigidbody>();
        //FirePoint = GameObject.Find("FirePoint");

        // Number of points to represent the trajectory
        //trajectoryRenderer.positionCount = 100;   
    }

    // Update is called once per frame
    void Update()
    {
        float turnAngle = steerInput.x * turnRate;
        float fireAngle = steerInput.y * turnRate;
        
        cannon_base.transform.Rotate(Vector3.up, turnAngle); //Turn the ship based on the horizontal input received
        cannon_tube.transform.Rotate(Vector3.left, fireAngle);

        SimulateTrajectory();
    }

    void OnCannonMove(InputValue value)
    {
        steerInput = value.Get<Vector2>(); //Store the new vector any time the move vector changes
    }

    void OnFire()
    {
        // init a new cannonball
        GameObject projectile_copy = Instantiate(projectile, FirePoint.transform.position, FirePoint.transform.rotation);

        //apply force
        Rigidbody rb = projectile_copy.GetComponent<Rigidbody>();

        rb.AddForce(cannon_tube.transform.forward * launchForce, ForceMode.Impulse);

        
    }

    void OnChangeCamera(InputValue value)
    {

    }


    private void SimulateTrajectory()
    {
        LineRenderer.positionCount = Mathf.CeilToInt(LinePoints / TimeBetweenPoints) + 1;
        Vector3 startPosition = FirePoint.localPosition; //this is wrong somehow :(
        //Debug.Log(startPosition);
        Vector3 startVelocity = launchForce * cannon_tube.transform.forward / projectile_rb.mass;

        int i = 0;
        LineRenderer.SetPosition(i, startPosition);
        for (float time = 0; time < LinePoints; time += TimeBetweenPoints)
        {
            i++;
            Vector3 point = startPosition + time * startVelocity;
            point.y = startPosition.y + startVelocity.y * time + (Physics.gravity.y / 2f * time * time);

            LineRenderer.SetPosition(i, point);
        }
    }




    //for (int i = 0; i < trajectoryPoints.Length; i++)
    /*{
         // Time interval for trajectory calculation
        //float time = i * Time.fixedDeltaTime;

        // need do be using init veloc here not launch force
        float x = launchForce * time * Mathf.Cos(launchAngleRadians);
        float y = (launchForce * time * Mathf.Sin(launchAngleRadians)) - 0.5f * (mass * g * time * time);
        trajectoryPoints[i] = new Vector3(x, y, 0f); // Store the calculated point
    }*/

    //https://youtu.be/8mGZBYsSXcQ line render

    //trajectoryRenderer.SetPositions(trajectoryPoints); // Update the Line Renderer positions
}

