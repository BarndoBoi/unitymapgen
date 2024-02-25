using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class Projectile : MonoBehaviour
{

    
    private Deform deform;

    

    // Start is called before the first frame update
    void Awake()
    {
       // look up casting
       deform = (Deform)GameObject.FindObjectOfType(typeof(Deform));
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnCollisionEnter(Collision collision)
    {
        Vector3 contact = collision.GetContact(0).point;
        Debug.Log(contact.ToString());
        deform.DeformTerrain(new Vector2(contact.z, contact.x), LayersEnum.Elevation);
        GameObject.Destroy(this.gameObject);
    }
}
