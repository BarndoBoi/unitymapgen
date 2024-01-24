using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class fishing_boat : MonoBehaviour
{

    public float speed = 5;
    public float turning_radius = 10f;

    


    // Start is called before the first frame update
    void Start()
    {
       
    }

    // Update is called once per frame
    void Update()
    {
        Vector2 input = new Vector2(Input.GetAxisRaw("Horizontal"),0);

        transform.position += transform.TransformDirection(Vector3.forward * speed * Time.deltaTime);
        //transform.Translate(transform.localPosition.z * speed * Time.deltaTime);
        transform.Rotate(Vector3.up, input.x * turning_radius);

        

    }
}

