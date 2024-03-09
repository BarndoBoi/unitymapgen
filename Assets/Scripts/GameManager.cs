using UnityEngine;
using ProcGenTiles;
using System.Linq;
using System.IO;
using System.Collections.Generic;
using UnityEngine.AI;

public class GameManager: MonoBehaviour
{
    [SerializeField]
    private LayerTerrain layerTerrain;

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

        layerTerrain.layersDict.Add(LayersEnum.Elevation, layerTerrain.elevationLayers);
        layerTerrain.layersDict.Add(LayersEnum.Moisture, layerTerrain.moistureLayers);

        layerTerrain.GenerateTerrain();
        
        X = layerTerrain.X;
        Y = layerTerrain.Y;
        
        LoadWaterShader();
        LoadMapBoundingBox();
        LoadNavMeshBuilder();

        LoadEnemyBoats();
        LoadPlayerBoat();

        
    }

    public void LoadTextures()
    {
        DirectoryInfo dir = new DirectoryInfo("Assets/Textures_and_Models/Resources/TerrainTextures/png");
        FileInfo[] info = dir.GetFiles("*.png"); //don't get the meta files
        int index = 0;
        List<TerrainLayer> layers = new List<TerrainLayer>();
        foreach (FileInfo file in info)
        {
            string fileName = Path.GetFileNameWithoutExtension(file.FullName);

            // Resources.Load() needs a 'Resources' folder, that's where it starts the search.
            string location_from_Resources_folder = "TerrainTextures/layers/";
            TerrainLayer texture = Resources.Load<TerrainLayer>(location_from_Resources_folder + fileName);
            layers.Add(texture);
            texturesDict.Add(fileName, index);
            index++;
        }
        terrain.terrainData.terrainLayers = layers.ToArray();

    }


    public void LoadWaterShader()
    {
        //waterMesh prefab is 50x50
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
        boundingQuad.localScale = new Vector3(layerTerrain.X, layerTerrain.X, layerTerrain.Y);
        Instantiate(boundingQuad, boundingQuad.position + new Vector3(X / 2, X / 2, Y), new Quaternion(0, 0, 0, 0), terrain.transform);
        Instantiate(boundingQuad, boundingQuad.position + new Vector3(X, X / 2, Y / 2), Quaternion.Euler(new Vector3(0, 90, 0)), terrain.transform);
        Instantiate(boundingQuad, boundingQuad.position + new Vector3(0, X / 2, Y / 2), Quaternion.Euler(new Vector3(0, 90, 0)), terrain.transform);
        boundingQuad.position = boundingQuad.position + new Vector3(X / 2, X / 2, 0);
    }

    public void LoadNavMeshBuilder()
    {
        Instantiate(navMeshBuilder, terrain.transform.position + new Vector3(X / 2, 0, Y / 2), new Quaternion(0, 0, 0, 0), terrain.transform);
        navMeshBuilder.m_Size = new Vector3(X, 50.0f, Y);
    }


    public void LoadEnemyBoats()
    {
        // This is how far from the origin point that SamplePosition will give a valid hit point
        float acceptableDistanceFromLand = 10f;

        int count = 0;
        string output = " ";
        // keep trying random points with NavMesh.SamplePosition() 
        // until we have X (numberOfEnemies) amount of Vector3 points for boats in the enemyBoatLoadPositions list
        while (enemyBoatLoadPositions.Count < numberOfEnemies)
        {
            int randomX = Random.Range(0, X);
            int randomY = Random.Range(0, Y);
            Vector3 randomPoint = new Vector3(randomX, 0, randomY);
            NavMeshHit hit;

            if (count < 25) Debug.Log(layerTerrain.finalMap.GetTile(randomX, randomY).ValuesHere[LayersEnum.Elevation]);
            count ++;



            if ((layerTerrain.finalMap.GetTile(randomX, randomY).ValuesHere[LayersEnum.Elevation] * layerTerrain.depth) < .05f) //use layerTerrain.highest_e
            {
                Debug.Log(layerTerrain.finalMap.GetTile(randomX, randomY).ValuesHere[LayersEnum.Elevation] * layerTerrain.depth);
                enemyBoatLoadPositions.Add(randomPoint);
            }



            /*if (NavMesh.SamplePosition(randomPoint, out hit, acceptableDistanceFromLand, 1)) //returns true and sets hit of the nearest navmesh point
            {
                if (layerTerrain.finalMap.GetTile((int)hit.position.x, (int)hit.position.z).ValuesHere[LayersEnum.Elevation] <= 0.01f) 
                {
                    Vector3 onTopWater = hit.position + new Vector3(0, waterHeight, 0);
                    enemyBoatLoadPositions.Add(onTopWater);
                }
                
            }*/
        }
        Debug.Log($"{count} iterations to get all points");
        Debug.Log("highest elevation is "+layerTerrain.depth);

        for (int i = 0; i < enemyBoatLoadPositions.Count; i++)
        {
            Instantiate(enemyBoat, enemyBoatLoadPositions[i], new Quaternion(0, 0, 0, 0), terrain.transform);
        }
    }


    public void LoadPlayerBoat()
    {

    }

}

