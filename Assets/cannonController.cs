using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class cannonController : MonoBehaviour
{
    /*
    This script assumes that you have a cannonball prefab, a fire point (the position where the cannonball will be spawned), and a launch force variable to control the projectile's speed. Attach this script to your cannon GameObject and assign the required variables in the Unity Editor.

Remember to replace cannonballPrefab with your cannonball prefab, and adjust the launchForce value based on your needs. Also, make sure the cannonball prefab has a Rigidbody2D component for physics interactions.

Feel free to modify the script according to your specific requirements.
    */
    public GameObject projectile;
    public Transform firePoint;
    public float launchForce = 10f;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            FireCannon();
        }
    }

    void FireCannon()
    {
        GameObject cannonball = Instantiate(projectile, projectile.transform.position, firePoint.rotation);
        Rigidbody2D rb = cannonball.GetComponent<Rigidbody2D>();

        if (rb != null)
        {
            rb.AddForce(firePoint.up * launchForce, ForceMode2D.Impulse);
        }
        else
        {
            Debug.LogError("Rigidbody2D component not found on cannonball prefab.");
        }
    }

}
