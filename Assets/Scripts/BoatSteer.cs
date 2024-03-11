using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class BoatSteer : MonoBehaviour
{

    [SerializeField]
    float acceleration = 0.05f;
    [SerializeField]
    float turnRate = 0.7f;

    [Range(-1, 4)]
    int throttle = 0;
    float speed = 0.0f;

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
        

        if (throttle > 0)
        {   if (speed <= throttle) speed += acceleration * throttle * Time.deltaTime; else speed -= acceleration * throttle * Time.deltaTime;
            transform.position += transform.forward * (speed * .05f);
        }

        if (throttle < 0)
        {
            if (speed <= throttle) speed -= acceleration * throttle * Time.deltaTime; else speed += acceleration * throttle * Time.deltaTime;
            transform.position += transform.forward * (speed * .05f);
        }

        if (throttle == 0)
        {
            if (speed > throttle) speed -= acceleration * Time.deltaTime; else speed += acceleration * Time.deltaTime;
            transform.position += transform.forward * (speed * .05f);
        }


     /*   if (speed < throttle)
        {
            speed += acceleration * Time.deltaTime;
        }
        if (throttle != 0) transform.position += transform.forward * (throttle * speed * .05f);*/


    }

    void OnMove(InputValue value)
    {
        int oldThrottle = throttle;

        steerInput = value.Get<Vector2>();
        throttle = Mathf.Clamp(throttle + (int)steerInput.y, -1 ,4 );
        
        if (oldThrottle != throttle) Debug.Log(throttle);

    }

    void OnChangeCamera(InputValue value)
    {

    }
}
