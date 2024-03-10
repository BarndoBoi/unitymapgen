using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class Projectile : MonoBehaviour
{


    private Deform deform;

    private TerrainCollider terrainCollider;

    private LayerTerrain layerTerrain;


    private Biomes biomes;

    private float waterheight_int;

    // Start is called before the first frame update
    void Awake()
    {
        // google en passant
        deform = (Deform)GameObject.FindObjectOfType(typeof(Deform));
        terrainCollider = (TerrainCollider)GameObject.FindObjectOfType(typeof(TerrainCollider));
        biomes = (Biomes)GameObject.FindObjectOfType(typeof(Biomes));
        layerTerrain = (LayerTerrain)GameObject.FindObjectOfType(typeof(LayerTerrain));

    }

    private void Start()
    {
        /*float level_perc = biomes.GetWaterLayer().value;
        int max_height = layerTerrain.depth;*/
        waterheight_int = biomes.GetWaterLayer().value * layerTerrain.depth;
    }


    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnCollisionEnter(Collision collision)
    {
        Vector3 contact = collision.GetContact(0).point;

        Collider objectHit = collision.GetContact(0).otherCollider;


        
        //Debug.Log("name of mesh parent object is: "+ collision.gameObject.transform.parent.name);

        // If hits Terrain above water
        if (contact.y > waterheight_int & objectHit == terrainCollider) //and not bounding box
        {
            deform.DeformTerrain(new Vector2(contact.z, contact.x), LayersEnum.Elevation);
        }

        if (collision.gameObject.name == "mesh") 
        {
            GameObject meshParentGameObject = collision.gameObject.transform.parent.gameObject;
            string meshParentGameObject_name = collision.gameObject.transform.parent.name;

            if (meshParentGameObject_name == "Enemy(Clone)" || meshParentGameObject_name == "Enemy")
            {
                meshParentGameObject.GetComponent<EnemyBoat>().TakeDamage(1);
            }
        }

        GameObject.Destroy(this.gameObject);
    }
}
