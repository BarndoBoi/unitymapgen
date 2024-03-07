using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class Projectile : MonoBehaviour
{

    
    private Deform deform;

    private Transform projectile;

    [SerializeField]
    private TerrainCollider terrainCollider;

    private LayerTerrain lt;
    

    // Start is called before the first frame update
    void Awake()
    {
       // look up casting
       deform = (Deform)GameObject.FindObjectOfType(typeof(Deform));
       terrainCollider = (TerrainCollider)GameObject.FindObjectOfType(typeof(TerrainCollider));
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnCollisionEnter(Collision collision)
    {
        Vector3 contact = collision.GetContact(0).point;
        //Debug.Log("got collision at: "+contact.ToString());
        Collider objectHit = collision.GetContact(0).otherCollider;
        Debug.Log("got collision with a: " + collision.GetContact(0).otherCollider.ToString());

        float waterheight = 3;
        
        if (contact.y > waterheight & objectHit == terrainCollider) //and not bounding box
        {
            deform.DeformTerrain(new Vector2(contact.z, contact.x), LayersEnum.Elevation);
        }

        GameObject.Destroy(this.gameObject);
    }
}
