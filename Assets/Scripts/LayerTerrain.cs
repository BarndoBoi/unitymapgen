using UnityEngine;
using ProcGenTiles;
using System.Linq;
using System.IO;
using System.Collections.Generic;
using UnityEngine.AI;

public class LayerTerrain : MonoBehaviour
{
    /* I want to create a map with the noise library and set a TerrainData up for rendering the backing data.
    // I'll use the same method of putting the heights in an "Elevation" layer and using that to put the islands together.
    // Then I'll likely want to check my Pathfinding code and see if I can check for regions that arent reachable and start marking them.
    // It would be cool to add canals or valleys between the water regions so everything is accessable by boat. :3
    */

    [SerializeField]
    private Biomes biomes;

    //Future TODO: Standardize these naming conventions between the ProcGenTiles library and our codebase
    [SerializeField]
    private int X;
    [SerializeField]
    private int Y;
    [SerializeField]
    private int depth; //Maybe rename to height instead? depth is kinda lame
    [SerializeField]
    private float noiseScale; //For transforming the int coords into smaller float values to sample the noise better. Functions as zoom in effect

    //Assign layers from the inspector. In the future I either want ScriptableObjects that can be dragged in or JSON serialization so these don't get lost on a reset
    [SerializeField]
    private MapLayers elevationLayers;
    [SerializeField]
    public MapLayers moistureLayers;

    [SerializeField]
    private FastNoiseLite noise;
    [SerializeField]
    private Terrain terrain; //This may become a custom mesh in the future, gotta dig up some code on it


    
    /// ////////////////////

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






    public Map finalMap { get; private set; } //This is where all of the layers get combined into.
    private Pathfinding pathfinding;

    public Dictionary<string, MapLayers> layersDict = new Dictionary<string, MapLayers>();

    public Renderer targetRenderer;
    //textures dict
    public Dictionary<string, int> texturesDict;

    float highest_e = -100;
    float lowest_e = 100;

    [SerializeField]
    public float waterHeight = 3f;

    // ----------------- DEBUG STUFF
    bool print_debug = false;
    

    

    public void Awake()
    {
        LoadTextures();
        LoadWaterShader();
        LoadMapBoundingBox();
        LoadNavMeshBuilder();

        LoadEnemyBoats();
        LoadPlayerBoat();

        if (terrain == null)
            terrain = GetComponent<Terrain>(); //Should already be assigned, but nab it otherwise

        //waterMesh = terrain.GetComponent<GameObject>();
        
        layersDict.Add(LayersEnum.Elevation, elevationLayers);
        layersDict.Add(LayersEnum.Moisture, moistureLayers);


        GenerateTerrain();
    }

    public void GenerateBiome()
    {   // THIS IS ALL GETTING MOVED TO ITS OWN SCRIPT


        //finalMap = new Map(X, Y); //Change this to only create a new map if the sizes differ. It might be getting garbe collected each time, and there's no reason
        for (int i = 0; i < moistureLayers.NoisePairs.Count; i++)
        {
            MapNoisePair pair = moistureLayers.NoisePairs[i];
            if (pair.UseJsonFile)
            {
                pair.NoiseParams = JsonUtility.FromJson<NoiseParams>(pair.JSON.text);
            }
            ReadNoiseParams(pair.NoiseParams); //Feed the generator this layer's info
            GenerateHeightmap(pair, LayersEnum.Moisture); //This function handles adding the layer into the finalMap, but it's not very clear. Needs cleaning up to be more readable
        }
        NormalizeFinalMap(LayersEnum.Moisture, 0, 1); //Make the final map only span from 0 to 1
        //CreateTerrainFromHeightmap();
    }

    public void GenerateTerrain()
    {
        finalMap = new Map(X, Y); //Change this to only create a new map if the sizes differ. It might be getting garbe collected each time, and there's no reason
        pathfinding = new Pathfinding(finalMap); //Init the pathfinding for adjusting regions after they're created
        for (int i = 0; i < elevationLayers.NoisePairs.Count; i++)
        {
            MapNoisePair pair = elevationLayers.NoisePairs[i];
            if (pair.UseJsonFile)
            {
                pair.NoiseParams = JsonUtility.FromJson<NoiseParams>(pair.JSON.text);
            }
            ReadNoiseParams(pair.NoiseParams); //Feed the generator this layer's info

            //if (i == 0) { minValue = pair.NoiseParams.minValue; raisedPower = pair.NoiseParams.raisedPower; };

            GenerateHeightmap(pair, LayersEnum.Elevation); //This function handles adding the layer into the finalMap, but it's not very clear. Needs cleaning up to be more readable
        }
        NormalizeFinalMap(LayersEnum.Elevation, elevationLayers.NoisePairs[0].NoiseParams.minValue, elevationLayers.NoisePairs[0].NoiseParams.raisedPower); //Make the final map only span from 0 to 1
        GenerateBiome();
        //biomes.GenerateBiomes();
        CreateTerrainFromHeightmap();
        //pathfinding.LandWaterFloodfill(0, 0, allBiomes);
        //pathfinding.MarkAllRegions();
        Debug.Log($"Number of regions marked: {pathfinding.regionSizes.Keys.Count}");
        for (int i = 0; i < pathfinding.regionSizes.Count; i++)
        {
            Debug.Log($"Region {i} contains {pathfinding.regionSizes[i]} tiles");
        }
    }

    public void CreateTerrainFromHeightmap()
    {
        TerrainData terrainData = terrain.terrainData;
        terrainData.size = new Vector3(X, depth, Y);
        terrainData.heightmapResolution = X + 1;
        terrainData.SetHeights(0, 0, finalMap.FetchFloatValues(LayersEnum.Elevation)); //SetHeights, I hate you so much >_<
        ApplyTextures(0,0,terrainData.alphamapHeight, terrainData.alphamapWidth, false);
    }

    public void ApplyTextures(int start_x, int start_y, int end_x, int end_y, bool deform)
    {
        TerrainData terrainData = terrain.terrainData;
        float[,,] splatmapData = new float[end_x-start_x, end_y-start_y, terrainData.alphamapLayers]; //Black magic fuckery, investigate more later
        //Debug.Log("NUMBER OF LAYERS IS  "+terrainData.alphamapLayers);
        for (int y = start_y; y < end_y; y++)
        {
            for (int x = start_x; x < end_x; x++)
            {
                float elevation = finalMap.GetTile(x, y).ValuesHere[LayersEnum.Elevation];
                float moisture = finalMap.GetTile(x, y).ValuesHere[LayersEnum.Moisture]; 

                // Setup an array to record the mix of texture weights at this point
                // TODO: see LoadTextures(), need to make TerrainLayer for each texture, so that it is added to the terrainData as a useable layer.
                float[] splatWeights = new float[terrainData.alphamapLayers];

                biome(); //sets the biome

                // Work in progress don't @ me
                void biome()
                {
                    if (elevation <= biomes.AllBiomes.values[0].value) { SetTexture("Water"); return; };
                    if (elevation < biomes.AllBiomes.values[1].value) { SetTexture("Sand"); return; };

                    if (elevation < biomes.AllBiomes.values[2].value) //if in grass band
                    {
                        if (moisture < .25f)
                        {
                            SetTexture("Dirt"); return;
                        }
                        SetTexture("Grass"); return; // else set to grass
                    };

                    if (elevation < biomes.AllBiomes.values[3].value) { SetTexture("Snow"); return; }; //snow

                    void SetTexture(string name)
                    {
                        splatWeights[texturesDict[name]] = 1.0f;
                    }
                }

                // Loop through each terrain texture
                for (int i = 0; i < terrainData.alphamapLayers; i++)
                {
                    // Assign this point to the splatmap array
                    splatmapData[x-start_x, y-start_y, i] = splatWeights[i];
                }
            }
        }
        terrainData.SetAlphamaps(start_y, start_x, splatmapData); //I have a feeling that this is what is making this function so slow. Need to profile it

        
    
    }
       
    public void ReadNoiseParams(NoiseParams noiseParams)
    {
        //Read the noise info from the MapLayer and set all of the FastNoiseLite fields here
        if (noise == null)
            noise = new FastNoiseLite();

        noise.SetSeed(noiseParams.seed);
        noise.SetNoiseType(noiseParams.noiseType);
        noise.SetFractalType(noiseParams.fractalType);
        noise.SetFractalGain(noiseParams.fractalGain);
        noise.SetFractalLacunarity(noiseParams.fractalLacunarity);
        noise.SetFractalOctaves(noiseParams.fractalOctaves);
        noise.SetFrequency(noiseParams.frequency);
        noise.SetFractalWeightedStrength(noiseParams.weightedStrength);
    }

    public void GenerateHeightmap(MapNoisePair noisePair, string layer)
    {
        highest_e = -100;
        lowest_e = 100;
        noisePair.Map = new Map(X, Y); //The map isn't being generated in the inspector, so it must be created here
        for (int x = 0; x < X; x++)
        {
            for (int y = 0; y < Y; y++)
            { //Inner for loop does most of the heavy lifting
                Tile tile = noisePair.Map.Tiles[x, y]; //Get the tile at the location
                float noiseValue = noise.GetNoise(x * noiseScale, y * noiseScale); //Grab the value between -1 and 1
                noiseValue = Mathf.InverseLerp(-1, 1, noiseValue); //set to 0  and 1 scale
                //noiseValue = Mathf.Pow(noiseValue, noisePair.NoiseParams.raisedPower); // TODO: do this AFTER the normalization


                //Set the elevation to the normalized value by checking if we've already set elevation data
                if (tile.ValuesHere.ContainsKey(layer))
                    tile.ValuesHere[layer] = noiseValue;
                else
                    tile.ValuesHere.Add(layer, noiseValue);
 
                Tile finalTile = finalMap.GetTile(x, y);

                if (finalTile.ValuesHere.ContainsKey(layer))
                { //If the value exist increment the final tile by the amount of the noise
                    finalTile.ValuesHere[layer] += noiseValue;
                }
                else
                { //Otherwise we add it with the value from the first layer
                    finalTile.ValuesHere.Add(layer, noiseValue); //Create the entry and assign the first layer's value
                }

                //track highest lowest values for later
                if (finalTile.ValuesHere[layer] < lowest_e) { lowest_e = finalTile.ValuesHere[layer]; };
                if (finalTile.ValuesHere[layer] > highest_e) { highest_e = finalTile.ValuesHere[layer]; };
            }
        }
    }

    public void NormalizeFinalMap(string layer, float minValue, float raisedPower)
    {
        float range = layersDict[layer].SumOfNoiseLayers();
        float lowest = 100;
        float highest = -100;

        float lowest_after = 100;
        float highest_after = -100;

        for (int x = 0; x < X; x++)
        {
            for (int y = 0; y < Y; y++)
            {
                Tile finalTile = finalMap.GetTile(x, y);
                
                // just for debug
                if (finalTile.ValuesHere[layer] < lowest) lowest = finalTile.ValuesHere[layer];
                if (finalTile.ValuesHere[layer] > highest) highest = finalTile.ValuesHere[layer];

                finalTile.ValuesHere[layer] = Mathf.InverseLerp(lowest_e, highest_e, finalTile.ValuesHere[layer]);
                if (layer == LayersEnum.Elevation)
                {
                    // TODO: do power stuff here not in GenTerrain()
                    finalTile.ValuesHere[layer] = Mathf.Pow(finalTile.ValuesHere[layer], raisedPower);

                    // TODO: do min-height stuff here, set anything below water level texture to water level.
                    //if (finalTile.ValuesHere[layer] < minValue) finalTile.ValuesHere[layer] = minValue;
                    // commented out becuz URP
                    //finalTile.ValuesHere[layer] = Mathf.Max(0, finalTile.ValuesHere[layer] - minValue);
                }

                // just for debug
                if (finalTile.ValuesHere[layer] < lowest_after) lowest_after = finalTile.ValuesHere[layer];
                if (finalTile.ValuesHere[layer] > highest_after) highest_after = finalTile.ValuesHere[layer];

            }
        }
        // uncomment for testing
        if (print_debug)
        {
            Debug.Log($"Lowest value before normalizing was {lowest} and highest was {highest} on {layer} layer ");
            Debug.Log($"Lowest value after normalizing was {lowest_after} and highest was {highest_after} on {layer} layer ");
        }
    }

    public void UpdateTerrainHeightmap(int xBase, int yBase, float[,] heightmap)
    { //This might need work to instead mark the terrain as dirty until all deform operations are done, and THEN we set the heights
        terrain.terrainData.SetHeights(xBase, yBase, heightmap); //Fuck you SetHeights, why do you pretend like I can update regions with the xBase and yBase when you actually suck?
                                                                 
                                                                 //Because fuck you, that's why! >:)
    }

    public void SerializeNoiseParamsToJson()
    { //For each NoiseParam in our layers we serialize them with the naming convention of layer + index in list
        for (int i = 0; i < elevationLayers.NoisePairs.Count; i++)
        {
            MapNoisePair pair = elevationLayers.NoisePairs[i];
            string json = pair.NoiseParams.SerializeParamsToJson(); // Get the JSON string

            // Define the path for the JSON file in the JSON folder inside the Assets folder
            string folderPath = Path.Combine(Application.dataPath, "JSON");
            string filePath = Path.Combine(folderPath, $"layer{i}.json");

            // Create the JSON folder if it doesn't exist
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            // Write the JSON string to the file
            File.WriteAllText(filePath, json);

            Debug.Log($"JSON file saved to: {filePath}");
        }
    }

    public void LoadNoiseParamsFromJson()
    {
        for (int i = 0; i < elevationLayers.NoisePairs.Count; i++)
        {
            MapNoisePair pair = elevationLayers.NoisePairs[i];
            if (pair.UseJsonFile && pair.JSON.text != string.Empty)
            { //Only load if the bool is on and the json asset is assigned
                pair.NoiseParams = JsonUtility.FromJson<NoiseParams>(pair.JSON.text);
            }
        }
    }
    
    

    public void LoadTextures()
    {
        texturesDict = new Dictionary<string, int>();

        DirectoryInfo dir = new DirectoryInfo("Assets/Textures_and_Models/Resources/TerrainTextures/png");
        FileInfo[] info = dir.GetFiles("*.png"); //don't get the meta files
        int index = 0;
        List<TerrainLayer> layers = new List<TerrainLayer>();
        foreach (FileInfo file in info )
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
        waterMesh.position = waterMesh.position + new Vector3(X/2, waterHeight, Y/2);
        waterMesh.localScale = new Vector3(X/waterMeshSize, 1, Y/waterMeshSize);

        //also load in the invisible plane at the water level, because that's what we're baking the navmesh on :)
        //Plane is 10x10
        waterMeshPlane.position = waterMeshPlane.position + new Vector3(X / 2, waterHeight, Y / 2);
        waterMeshPlane.localScale = new Vector3(X/10, 1, Y/10);
    }

    public void LoadMapBoundingBox()
    {
        //Quad (vertical and flat square) is 1x1
        // has the origin in the center of the square
        // public static Object Instantiate(Object original, Vector3 position, Quaternion rotation, Transform parent);
        boundingQuad.localScale = new Vector3(X, X, Y);
        Instantiate(boundingQuad, boundingQuad.position + new Vector3(X/2, X/2, Y), new Quaternion(0, 0, 0, 0), terrain.transform);
        Instantiate(boundingQuad, boundingQuad.position + new Vector3(X, X / 2, Y/2), Quaternion.Euler(new Vector3(0,90,0)), terrain.transform);
        Instantiate(boundingQuad, boundingQuad.position + new Vector3(0, X / 2, Y/ 2), Quaternion.Euler(new Vector3(0, 90, 0)), terrain.transform);
        boundingQuad.position = boundingQuad.position + new Vector3(X/2, X/2, 0);
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
                  
                Debug.Log($"got good pos at  " + hit.position);
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
