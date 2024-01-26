using UnityEngine;

public class fishing_boat : MonoBehaviour
{

    public float speed = 25f;
    public float turning_radius = .7f;

    public float launchForce = 25f;

    private GameObject cannon_tube;
    private GameObject cannon_base;
    private GameObject projectile;
    private GameObject FirePoint;

    public GameObject MainCamera;
    public GameObject OverviewCamera;
    public GameObject FollowCamera;

    public string currCamera;


    // Start is called before the first frame update
    void Start()
    {
        cannon_tube = GameObject.Find("cannon_tube");
        cannon_base = GameObject.Find("cannon_base");
        projectile = GameObject.Find("projectile");
        FirePoint = GameObject.Find("FirePoint");

        //MainCamera = GameObject.Find("MainCamera");
        //OverviewCamera = GameObject.Find("OverviewCamera");
        MainCamera.SetActive(true);
        OverviewCamera.SetActive(false);
        currCamera = "MainCamera";


    }

    // Update is called once per frame
    void Update()
    {
        Vector2 input = new Vector2(Input.GetAxisRaw("Horizontal"),0);
        Vector2 input2 = new Vector2(Input.GetAxisRaw("Vertical"), 0);
        if (input2.x > 0) { transform.position += transform.TransformDirection(Vector3.forward * speed * Time.deltaTime); };
        //transform.Translate(transform.localPosition.z * speed * Time.deltaTime);
        transform.Rotate(Vector3.up, input.x * turning_radius);
                
        // rotates cannon tube with H and J
        if (Input.GetKey(KeyCode.H)) { cannon_tube.transform.Rotate(Vector3.left, turning_radius); };
        if (Input.GetKey(KeyCode.J)) { cannon_tube.transform.Rotate(Vector3.right, turning_radius); };

        // rotates cannon base with K and L
        if (Input.GetKey(KeyCode.K)) { cannon_base.transform.Rotate(Vector3.up, turning_radius); };
        if (Input.GetKey(KeyCode.L)) { cannon_base.transform.Rotate(Vector3.down, turning_radius); };

        // fire cannon with F
        // it's firing multiple, need to only let it fire one 
        if (Input.GetKeyUp(KeyCode.F)) { FireCannon(); }

        //swap between cameras
        if (Input.GetKeyUp(KeyCode.C))
        {
            if (currCamera == "MainCamera")
            {
                MainCamera.SetActive(false);
                OverviewCamera.SetActive(true);
                currCamera = "OverviewCamera";
            } 
            else
            {
                MainCamera.SetActive(true);
                OverviewCamera.SetActive(false);
                currCamera = "MainCamera";
            }
        
            
        }
    }

    void FireCannon() //THIS CODE IS FUCKED, I'M WORKING ON IT 
                      //Idk how to make it fire in the cannon's direction
    {   
        // init a new cannonball
        GameObject projectile_copy = Instantiate(projectile, FirePoint.transform.position, FirePoint.transform.rotation);

        //apply force
        Rigidbody rb = projectile_copy.GetComponent<Rigidbody>();

        rb.AddForce(cannon_tube.transform.forward * launchForce, ForceMode.Impulse);
    
    }
}

