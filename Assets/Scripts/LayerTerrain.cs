using UnityEngine;
using ProcGenTiles;
using System.Linq;
using System.IO;
using System.Collections.Generic;

public class LayerTerrain : MonoBehaviour
{
    /* I want to create a map with the noise library and set a TerrainData up for rendering the backing data.
    // I'll use the same method of putting the heights in an "Elevation" layer and using that to put the islands together.
    // Then I'll likely want to check my Pathfinding code and see if I can check for regions that arent reachable and start marking them.
    // It would be cool to add canals or valleys between the water regions so everything is accessable by boat. :3
    */

    [SerializeField]
    private Biomes allBiomes;

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
    private MapLayers moistureLayers;

    [SerializeField]
    private FastNoiseLite noise;
    [SerializeField]
    private Terrain terrain; //This may become a custom mesh in the future, gotta dig up some code on it

    public Map finalMap { get; private set; } //This is where all of the layers get combined into.

    public Dictionary<string,MapLayers> layersDict = new Dictionary<string, MapLayers>();

    float highest_e = -100;
    float lowest_e = 100;

    // ----------------- DEBUG STUFF
    bool print_debug = false;
    

    

    public void Awake()
    {
        if (terrain == null)
            terrain = GetComponent<Terrain>(); //Should already be assigned, but nab it otherwise
        
        layersDict.Add(LayersEnum.Elevation, elevationLayers);
        layersDict.Add(LayersEnum.Moisture, moistureLayers);

        GenerateTerrain();
    }

    public void doBiomeStuff()
    {   
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
        float minValue = 0;
        float raisedPower = 0;
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
        doBiomeStuff();
        CreateTerrainFromHeightmap();
    }

    public void CreateTerrainFromHeightmap()
    {
        TerrainData terrainData = terrain.terrainData;
        terrainData.size = new Vector3(X, depth, Y);
        terrainData.heightmapResolution = X + 1;
        terrainData.SetHeights(0, 0, finalMap.FetchFloatValues(LayersEnum.Elevation)); //SetHeights, I hate you so much >_<

        float[,,] splatmapData = new float[terrainData.alphamapWidth, terrainData.alphamapHeight, terrainData.alphamapLayers]; //Black magic fuckery, investigate more later

        float maxH = 0; //for printing later
        float minH = .99f; 
        for (int y = 0; y < terrainData.alphamapWidth; y++)
        {
            for (int x = 0; x < terrainData.alphamapHeight; x++)
            {           
                float height = finalMap.GetTile(x,y).ValuesHere[LayersEnum.Elevation];
                float biomeHeight = finalMap.GetTile(x, y).ValuesHere[LayersEnum.Moisture];
                //if (height > maxH) { maxH = height; };
                //if (height < minH) { minH = height; };

                // Setup an array to record the mix of texture weights at this point
                float[] splatWeights = new float[terrainData.alphamapLayers];
                
                biome(); //sets the biome

                // Work in progress don't @ me
                void biome()
                {   
                    if (height <= allBiomes.AllBiomes[0].value) { splatWeights[allBiomes.AllBiomes[0].index] = 1.0f; return; }; // set water (constant)
                    if (height < allBiomes.AllBiomes[1].value) { splatWeights[allBiomes.AllBiomes[1].index] = 1.0f; return; }; // set beach sand (constant)

                    if (height < allBiomes.AllBiomes[2].value) //if in grass band
                    {
                        if (biomeHeight < .25f) //set to dirt
                        {
                            splatWeights[allBiomes.AllBiomes[4].index] = 1.0f; return;
                        }

                        splatWeights[allBiomes.AllBiomes[2].index] = 1.0f; return; // else set to grass
                    };

                    if (height < allBiomes.AllBiomes[3].value) { splatWeights[allBiomes.AllBiomes[3].index] = 1.0f; return; }; //snow


                        splatWeights[allBiomes.AllBiomes[2].index] = 1.0f; return; // else set to grass
                    };

                }


                // Sum of all textures weights must add to 1, so calculate normalization factor from sum of weights
                float z = splatWeights.Sum();

                // Loop through each terrain texture
                for (int i = 0; i < terrainData.alphamapLayers; i++)
                {
                    // Normalize so that sum of all texture weights = 1
                    splatWeights[i] /= z;

                    // Assign this point to the splatmap array
                    splatmapData[x, y, i] = splatWeights[i]; 
                }
            }
        }
        terrainData.SetAlphamaps(0, 0, splatmapData); //I have a feeling that this is what is making this function so slow. Need to profile it
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

    void NormalizeFinalMap(string layer, float minValue, float raisedPower)
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
                    finalTile.ValuesHere[layer] = Mathf.Max(0, finalTile.ValuesHere[layer] - minValue);
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
}