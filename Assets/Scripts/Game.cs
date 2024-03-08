using UnityEngine;
using ProcGenTiles;
using System.Linq;
using System.IO;
using System.Collections.Generic;
using UnityEngine.AI;

public class Game : MonoBehaviour
{
    [SerializeField]
    private LayerTerrain lt;

    private int X;
    private int Y;
  
    [SerializeField]
    private Terrain terrain; //This may become a custom mesh in the future, gotta dig up some code on it

    public Dictionary<string, int> texturesDict = new Dictionary<string, int>();

    [SerializeField]
    private GameObject waterPrefab;
    [SerializeField]
    private Transform waterMeshPlane;
    [SerializeField]
    private Transform boundingQuad;
    [SerializeField]
    private LocalNavMeshBuilder navMeshBuilder;

    [SerializeField]
    private int numberOfEnemies;
    public static List<Vector3> enemyBoatLoadPositions = new List<Vector3>();

    [SerializeField]
    public GameObject enemyBoat;

    public EnemyBoat enemy;


    [SerializeField]
    public float waterHeight = 3f;


    public void Awake()
    {
        if (terrain == null)
            terrain = GetComponent<Terrain>(); //Should already be assigned, but nab it otherwise

        LoadTextures();

        lt.layersDict.Add(LayersEnum.Elevation, lt.elevationLayers);
        lt.layersDict.Add(LayersEnum.Moisture, lt.moistureLayers);

        lt.GenerateTerrain();
        
        X = lt.X;
        Y = lt.Y;

        //LoadTextures();
        LoadWaterShader();
        LoadMapBoundingBox();
        LoadNavMeshBuilder();

        LoadEnemyBoats();
        LoadPlayerBoat();

        
    }

    public void LoadTextures()
    {
        //texturesDict = new Dictionary<string, int>();

        DirectoryInfo dir = new DirectoryInfo("Assets/Textures_and_Models/Resources/TerrainTextures/png");
        FileInfo[] info = dir.GetFiles("*.png"); //don't get the meta files
        int index = 0;
        List<TerrainLayer> layers = new List<TerrainLayer>();
        foreach (FileInfo file in info)
        {
            string fileName = Path.GetFileNameWithoutExtension(file.FullName);

            // Resources.Load() needs a 'Resources' folder, that's where it starts the search.
            // The path here is Assets/Textures_and_Models/Resources/TerrainTextures/png/
            // but it only needs the info after the Resources folder (Resources/)

            string location_from_Resources_folder = "TerrainTextures/layers/";
            TerrainLayer texture = Resources.Load<TerrainLayer>(location_from_Resources_folder + fileName);
            layers.Add(texture);
            texturesDict.Add(fileName, index);
            index++;
        }
        terrain.terrainData.terrainLayers = layers.ToArray();

        // DEBUG
        /* foreach (KeyValuePair<string, int> kvp in texturesDict)
         {
             Debug.Log($"Key = '{kvp.Key}'   value = '{kvp.Value}'");
         }*/



    }


    public void LoadWaterShader()
    {
        //waterMesh is 50x50
        // has the origin in the center
        float waterMeshSize = 50f;
        float waterHeight = 3;
        GameObject prefab = GameObject.Instantiate(waterPrefab);
        Transform waterMesh = prefab.GetComponent<Transform>();
        waterMesh.position = waterMesh.position + new Vector3(X / 2, waterHeight, Y / 2);
        waterMesh.localScale = new Vector3(X / waterMeshSize, 1, Y / waterMeshSize);

        //also load in the invisible plane at the water level, because that's what we're baking the navmesh on :)
        //Plane is 10x10
        waterMeshPlane.position = waterMeshPlane.position + new Vector3(X / 2, waterHeight, Y / 2);
        waterMeshPlane.localScale = new Vector3(X / 10, 1, Y / 10);
    }

    public void LoadMapBoundingBox()
    {
        //Quad (vertical and flat square) is 1x1
        // has the origin in the center of the square
        // public static Object Instantiate(Object original, Vector3 position, Quaternion rotation, Transform parent);

        Debug.Log(boundingQuad);
        boundingQuad.localScale = new Vector3(lt.X, lt.X, lt.Y);
        Instantiate(boundingQuad, boundingQuad.position + new Vector3(X / 2, X / 2, Y), new Quaternion(0, 0, 0, 0), terrain.transform);
        Instantiate(boundingQuad, boundingQuad.position + new Vector3(X, X / 2, Y / 2), Quaternion.Euler(new Vector3(0, 90, 0)), terrain.transform);
        Instantiate(boundingQuad, boundingQuad.position + new Vector3(0, X / 2, Y / 2), Quaternion.Euler(new Vector3(0, 90, 0)), terrain.transform);
        boundingQuad.position = boundingQuad.position + new Vector3(X / 2, X / 2, 0);
    }

    public void LoadNavMeshBuilder()
    {
        // idk this shit don't work
        Instantiate(navMeshBuilder, terrain.transform.position + new Vector3(X / 2, 0, Y / 2), new Quaternion(0, 0, 0, 0), terrain.transform);
        //navMeshBuilder.transform.position = navMeshBuilder.transform.position + new Vector3(X / 2, 0, Y / 2);
        navMeshBuilder.m_Size = new Vector3(X, 50.0f, Y);

    }


    public void LoadEnemyBoats()
    {
        // This is how far from the origin point that SamplePosition will give a valid hit point
        float acceptableDistanceFromLand = 10f;

        /*
         There's a list up top called enemyBoatLoadPositions
        We keep trying random points with NavMesh.SamplePosition() until we have X (numberOfEnemies) amount of Vector3 points for boats in the list
         After we have the right amount, we instantiate them all.
        
        
         */
        while (enemyBoatLoadPositions.Count < numberOfEnemies)
        {
            Vector3 randomPoint = new Vector3(Random.Range(0, X), 4, Random.Range(0, Y));
            NavMeshHit hit;

            if (NavMesh.SamplePosition(randomPoint, out hit, acceptableDistanceFromLand, 1)) //returns true and sets hit of the nearest navmesh point
            {
                enemyBoatLoadPositions.Add(hit.position);
            }
        }

        for (int i = 0; i < enemyBoatLoadPositions.Count; i++)
        {
            Instantiate(enemyBoat, enemyBoatLoadPositions[i], new Quaternion(0, 0, 0, 0), terrain.transform);
        }
    }


    public void LoadPlayerBoat()
    {

    }

}

