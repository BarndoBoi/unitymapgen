using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class BoatSteer : MonoBehaviour
{
    [SerializeField]

    float acceleration = 0.20f;
    [SerializeField]
    float speed = 0.0f;

    [SerializeField]
    float turnRate = 0.05f;

    [SerializeField]
    [Range(-1, 4)]
    int throttle = 0;

    private Vector2 steerInput;
    

    // Update is called once per frame
    void Update()
    {
        float turnAngle = Mathf.Clamp(steerInput.x * (.75f / speed), -.5f, .5f); //this needs to be edited to be better but it works
        transform.Rotate(Vector3.up, turnAngle);

        //TODO: these can all be simplified down, I made things verbose for debugging
        if (throttle > 0)
        {
            if (speed <= throttle) speed += acceleration * throttle * Time.deltaTime; else speed -= acceleration * throttle * Time.deltaTime;
            transform.position += transform.forward * (speed * .05f);
        }

        if (throttle < 0)
        {
            if (speed <= throttle) speed -= acceleration * throttle * Time.deltaTime; else speed += acceleration * throttle * Time.deltaTime;
            transform.position += transform.forward * (speed * .05f);
        }

        if (throttle == 0)
        {
            if (speed > throttle) speed -= acceleration * Time.deltaTime;
            if (speed < throttle) speed += acceleration * Time.deltaTime;
            transform.position += transform.forward * (speed * .05f);
        }

    }

    void OnMove(InputValue value)
    {
        steerInput = value.Get<Vector2>();
        throttle = Mathf.Clamp(throttle + (int)steerInput.y, -1, 4);
    }

    void OnChangeCamera(InputValue value)
    {

    }
}
