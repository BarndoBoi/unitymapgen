using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class BoatSteer : MonoBehaviour
{

    [SerializeField]
    float speed = 0.5f;
    [SerializeField]
    float turnRate = 0.8f;
    [SerializeField]
    float minimumInput = 0.01f; //Can't go slower than this

    [Range(-1, 4)]
    int throttle = 0;

    private Vector2 steerInput;

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        //float turnAngle = steerInput.x * turnRate;
        float turnAngle = steerInput.x / throttle;
        transform.Rotate(Vector3.up, turnAngle); //Turn the ship based on the horizontal input received 
        transform.position += transform.forward * ((float)throttle * .1f);
        Debug.Log(throttle);

    }

    void OnMove(InputValue value)
    {
        steerInput = value.Get<Vector2>(); //Store the new vector any time the move vector changes
        throttle = Mathf.Clamp(throttle + (int)steerInput.y, -1,4);
        
    }

    void OnChangeCamera(InputValue value)
    {

    }
}
