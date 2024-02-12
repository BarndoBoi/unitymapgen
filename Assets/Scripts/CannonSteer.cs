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

    // Start is called before the first frame update
    void Start()
    {
        cannon_tube = GameObject.Find("cannon_tube");
        cannon_base = GameObject.Find("cannon_base");
        projectile = GameObject.Find("projectile");
        FirePoint = GameObject.Find("FirePoint");
    }

    // Update is called once per frame
    void Update()
    {
        float turnAngle = steerInput.x * turnRate;
        float fireAngle = steerInput.y * turnRate;
        
        cannon_base.transform.Rotate(Vector3.up, turnAngle); //Turn the ship based on the horizontal input received
        cannon_tube.transform.Rotate(Vector3.left, fireAngle);
        //transform.position += transform.forward * Mathf.Clamp(steerInput.y, minimumInput, float.MaxValue) * speed; //Can't sit still

        /*Ray ray = new Ray(transform.position, transform.InverseTransformDirection(Vector3.forward) * rayLength);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, rayLength))
        { //Do steering away from the cross product of the ray and the vector

            

            //Flatten vector3 to do dot product? Might need to just do whisker steering instead

        }*/
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
}
