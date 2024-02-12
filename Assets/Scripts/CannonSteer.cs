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

    [SerializeField]
    float launchForce = 5f;

    private Vector2 steerInput;

    public GameObject cannon_tube;
    public GameObject cannon_base;
    public GameObject projectile;
    public GameObject FirePoint;

    public LineRenderer trajectoryRenderer;

    // Start is called before the first frame update
    void Start()
    {
        cannon_tube = GameObject.Find("cannon_tube");
        cannon_base = GameObject.Find("cannon_base");
        projectile = GameObject.Find("projectile");
        FirePoint = GameObject.Find("FirePoint");
        
        // Number of points to represent the trajectory
        trajectoryRenderer.positionCount = 100;   
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


    void SimulateTrajectory()
    {
        Vector3[] trajectoryPoints = new Vector3[100]; // Array to store trajectory points
        float g = Physics.gravity.magnitude; // Magnitude of gravity

        float angle = cannon_tube.transform.rotation.x;
        float mass = 1f;

        for (int i = 0; i < trajectoryPoints.Length; i++)
        {
            float time = i * 0.1f; // Time interval for trajectory calculation
            float x = launchForce * time * Mathf.Cos(angle * Mathf.Deg2Rad);
            //float y = launchForce * time * Mathf.Sin(angle * Mathf.Deg2Rad) - 0.5f * g * time * time;
            float y = (launchForce * time * Mathf.Sin(angle * Mathf.Deg2Rad)) - 0.5f * (mass * g * time * time); //TODO get mass of rigidbody instead of setting manually
            trajectoryPoints[i] = new Vector3(x, y, 0f); // Store the calculated point
        }

        trajectoryRenderer.SetPositions(trajectoryPoints); // Update the Line Renderer positions
    }
}
