using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class BoatSteer : MonoBehaviour
{

    [SerializeField]
    float speed = 0.5f;
    [SerializeField]
    float turnRate = 0.7f;
    [SerializeField]
    float minimumInput = 0.01f; //Can't go slower than this

    private Vector2 steerInput;

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        float turnAngle = steerInput.x * turnRate;
        transform.Rotate(Vector3.up, turnAngle); //Turn the ship based on the horizontal input received
        transform.position += transform.forward * Mathf.Clamp(steerInput.y, minimumInput, float.MaxValue) * speed; //Can't sit still

        /*Ray ray = new Ray(transform.position, transform.InverseTransformDirection(Vector3.forward) * rayLength);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, rayLength))
        { //Do steering away from the cross product of the ray and the vector

            

            //Flatten vector3 to do dot product? Might need to just do whisker steering instead

        }*/
    }

    void OnMove(InputValue value)
    {
        steerInput = value.Get<Vector2>(); //Store the new vector any time the move vector changes
    }

    void OnChangeCamera(InputValue value)
    {

    }
}
