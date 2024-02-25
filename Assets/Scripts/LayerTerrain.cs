using UnityEngine;
using ProcGenTiles;
using System.Linq;
using System.IO;

public class LayerTerrain : MonoBehaviour
{
    /* I want to create a map with the noise library and set a TerrainData up for rendering the backing data.
    // I'll use the same method of putting the heights in an "Elevation" layer and using that to put the islands together.
    // Then I'll likely want to check my Pathfinding code and see if I can check for regions that arent reachable and start marking them.
    // It would be cool to add canals or valleys between the water regions so everything is accessable by boat. :3
    */

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
    private MapLayers mapLayers;
    [SerializeField]
    private FastNoiseLite noise;
    [SerializeField]
    private Terrain terrain; //This may become a custom mesh in the future, gotta dig up some code on it

    public Map finalMap { get; private set; } //This is where all of the layers get combined into.

    public void Awake()
    {
        if (terrain == null)
            terrain = GetComponent<Terrain>(); //Should already be assigned, but nab it otherwise
        GenerateTerrain();
    }

    public void GenerateTerrain()
    {
        //Finals array likely doesn't need to exist here since we have the finalMap field
        float[,] finals = new float[X, Y];
        finalMap = new Map(X, Y); //Change this to only create a new map if the sizes differ. It might be getting garbe collected each time, and there's no reason
        for (int i = 0; i < mapLayers.NoisePairs.Count; i++)
        {
            MapNoisePair pair = mapLayers.NoisePairs[i];
            if (pair.UseJsonFile)
            {
                pair.NoiseParams = JsonUtility.FromJson<NoiseParams>(pair.JSON.text);
            }
            ReadNoiseParams(pair.NoiseParams); //Feed the generator this layer's info
            GenerateHeightmap(pair, finals); //This function handles adding the layer into the finalMap, but it's not very clear. Needs cleaning up to be more readable

        }
        CreateTerrainFromHeightmap(finals);
    }

    public void CreateTerrainFromHeightmap(float[,] heights)
    {
        TerrainData terrainData = terrain.terrainData;
        terrainData.size = new Vector3(X, depth, Y);
        terrainData.heightmapResolution = X + 1;
        //This needs to be changed to use FetchFloatValues instead since we can't update a region of the Terrain
        float[,] testHeights = finalMap.FetchFloatValuesSlice(LayersEnum.Elevation, 0, Y, 0, X);
        terrainData.SetHeights(0, 0, testHeights); //SetHeights, I hate you so much >_<
        float[,,] splatmapData = new float[terrainData.alphamapWidth, terrainData.alphamapHeight, terrainData.alphamapLayers]; //Black magic fuckery, investigate more later

        for (int y = 0; y < terrainData.alphamapWidth; y++)
        {
            for (int x = 0; x < terrainData.alphamapHeight; x++)
            {

                float y_01 = (float)y / (float)terrainData.alphamapWidth;
                float x_01 = (float)x / (float)terrainData.alphamapHeight;

                float height = terrainData.GetHeight(Mathf.RoundToInt(x_01 * terrainData.heightmapResolution),
                Mathf.RoundToInt(y_01 * terrainData.heightmapResolution));

                // Setup an array to record the mix of texture weights at this point
                float[] splatWeights = new float[terrainData.alphamapLayers];

                //All of the biome code needs to be removed from here and put into a serizlizable data object

                splatWeights[0] = 0.0f; //They all are already initialized to zero, these assignments are pointless
                splatWeights[1] = 0.0f;
                splatWeights[2] = 0.0f;
                splatWeights[3] = 0.0f;

                float hm_perc = (height / terrainData.heightmapResolution) * 10f; //Offsetting by ten to get a percentage? Double check this in testing

                Biome(); //sets the biome

                void Biome() //This needs to be refactored badly
                {
                    // will need to tune further but this will work for the basics now. 
                    // RedBlob had a better implementation of this that might be worth looking into
                    if (hm_perc < 0.1) { splatWeights[2] = 1.0f; return; } //water
                    if (hm_perc < 0.15) { splatWeights[0] = 1.0f; return; } //beach sand
                    if (hm_perc < 0.65) { splatWeights[1] = 1.0f; return; } // grass
                    if (hm_perc >= 0.65) { splatWeights[3] = 1.0f; return; } //snow

                }

                // Sum of all textures weights must add to 1, so calculate normalization factor from sum of weights
                float z = splatWeights.Sum();

                // Loop through each terrain texture
                for (int i = 0; i < terrainData.alphamapLayers; i++)
                {
                    // Normalize so that sum of all texture weights = 1
                    splatWeights[i] /= z;

                    // Assign this point to the splatmap array
                    splatmapData[y, x, i] = splatWeights[i];
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

    public float[,] GenerateHeightmap(MapNoisePair noisePair, float[,] final)
    {
        noisePair.Map = new Map(X, Y); //The map isn't being generated in the inspector, so it must be created here
        float[,] heightmap = new float[X, Y];
        for (int x = 0; x < X; x++)
        {
            for (int y = 0; y < Y; y++)
            { //Inner for loop does most of the heavy lifting
                Tile tile = noisePair.Map.Tiles[x, y]; //Get the tile at the location
                float noiseValue = noise.GetNoise(x * noiseScale, y * noiseScale) / 2 + 0.5f; //Grab the value
                noiseValue = Mathf.Pow(noiseValue, noisePair.NoiseParams.raisedPower); //raising to power to give us flat valleys for ocean floor
                //Set the elevation to the normalized value by checking if we've already set elevation data
                if (tile.ValuesHere.ContainsKey(LayersEnum.Elevation))
                    tile.ValuesHere[LayersEnum.Elevation] = noiseValue;
                else
                    tile.ValuesHere.Add(LayersEnum.Elevation, noiseValue);

                
                heightmap[x, y] = noiseValue;
                //No need to carry around a final array since we can simply call FetchFloatValues on finalMap
                final[x, y] += noiseValue; //Add the layers values to the final heightmap array
                Tile finalTile = finalMap.Tiles[x, y];
                if (finalTile.ValuesHere.ContainsKey(LayersEnum.Elevation))
                { //If the value exist increment the final tile by the amount of the noise
                    finalTile.ValuesHere[LayersEnum.Elevation] += noiseValue;
                }
                else
                { //Otherwise we add it with the value from the first layer
                    finalTile.ValuesHere.Add(LayersEnum.Elevation, noiseValue); //Create the entry and assign the first layer's value
                }
            }
        }
        return heightmap;
    }

    public void UpdateTerrainHeightmap(int xBase, int yBase, float[,] heightmap)
    { //This might need work to instead mark the terrain as dirty until all deform operations are done, and THEN we set the heights
        terrain.terrainData.SetHeights(xBase, yBase, heightmap); //Fuck you SetHeights, why do you pretend like I can update regions with the xBase and yBase when you actually suck?
    }

    public void SerializeNoiseParamsToJson()
    { //For each NoiseParam in our layers we serialize them with the naming convention of layer + index in list
        for (int i = 0; i < mapLayers.NoisePairs.Count; i++)
        {
            MapNoisePair pair = mapLayers.NoisePairs[i];
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
}