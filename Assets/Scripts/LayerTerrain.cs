using UnityEngine;
using ProcGenTiles;
using System.Linq;

public class LayerTerrain : MonoBehaviour
{
    /* I want to create a map with the noise library and set a TerrainData up for rendering the backing data.
    // I'll use the same method of putting the heights in an "Elevation" layer and using that to put the islands together.
    // Then I'll likely want to check my Pathfinding code and see if I can check for regions that arent reachable and start marking them.
    // It would be cool to add canals or valleys between the water regions so everything is accessable by boat. :3
    */
    [SerializeField]
    private int width;
    [SerializeField]
    private int height;
    [SerializeField]
    private int depth;
    [SerializeField]
    private float noiseScale; //For transforming the int coords into smaller float values to sample the noise better

    [SerializeField]
    private MapLayers mapLayers;
    [SerializeField]
    private FastNoiseLite noise;
    [SerializeField]
    private Terrain terrain;

    public Map finalMap { get; private set; } //This is where all of the layers get combined into

    public void Start()
    {
        if (terrain == null)
            terrain = GetComponent<Terrain>(); //Should already be assigned, but nab it otherwise
        GenerateTerrain();
    }

    public void GenerateTerrain()
    {
        float[,] finals = new float[width, height];
        finalMap = new Map(width, height);
        for (int i = 0; i < mapLayers.NoisePairs.Count; i++)
        {
            MapNoisePair pair = mapLayers.NoisePairs[i];
            ReadNoiseParams(pair.NoiseParams); //Feed the generator this layer's info
            float[,] heights = GenerateHeightmap(pair, finals);

        }
        CreateTerrainFromHeightmap(finals);
    }

    public void CreateTerrainFromHeightmap(float[,] heights)
    {
        TerrainData terrainData = terrain.terrainData;
        terrainData.size = new Vector3(width, depth, height);
        terrainData.heightmapResolution = width + 1;
        terrainData.SetHeights(0, 0, heights);
        float[,,] splatmapData = new float[terrainData.alphamapWidth, terrainData.alphamapHeight, terrainData.alphamapLayers];

        for (int y = 0; y < terrainData.alphamapHeight; y++)
        {
            for (int x = 0; x < terrainData.alphamapWidth; x++)
            {

                float y_01 = (float)y / (float)terrainData.alphamapHeight;
                float x_01 = (float)x / (float)terrainData.alphamapWidth;

                float height = terrainData.GetHeight(Mathf.RoundToInt(x_01 * terrainData.heightmapResolution),
                Mathf.RoundToInt(y_01 * terrainData.heightmapResolution));

                // not using these right now
                // Vector3 normal = terrainData.GetInterpolatedNormal(y_01, x_01);
                // float steepness = terrainData.GetSteepness(y_01, x_01);

                // Setup an array to record the mix of texture weights at this point
                float[] splatWeights = new float[terrainData.alphamapLayers];

                splatWeights[0] = 0.0f;
                splatWeights[1] = 0.0f;
                splatWeights[2] = 0.0f;
                splatWeights[3] = 0.0f;

                // the percent of terrains max height that this area is
                //Debug.Log(height + "   " + terrainData.heightmapResolution);
                float hm_perc = (height / terrainData.heightmapResolution) * 10f;

                Biome(); //sets the biome

                void Biome()
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
        terrainData.SetAlphamaps(0, 0, splatmapData);
    }

    public void ReadNoiseParams(NoiseParams noiseParams)
    {
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
        noisePair.Map = new Map(width, height);
        float[,] heightmap = new float[width, height];
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            { //Inner for loop does most of the heavy lifting
                Tile tile = noisePair.Map.Tiles[i, j]; //Get the tile at the location
                float noiseValue = noise.GetNoise(i * noiseScale, j * noiseScale) / 2 + 0.5f; //Grab the value
                noiseValue = Mathf.Pow(noiseValue, noisePair.NoiseParams.raisedPower); //raising to power to give us flat valleys for ocean floor
                //v = Mathf.InverseLerp(-1, 1, v); //Normalize the returned noise
                //Set the elevation to the normalized value by checking if we've already set elevation data
                if (tile.ValuesHere.ContainsKey(LayersEnum.Elevation))
                    tile.ValuesHere[LayersEnum.Elevation] = noiseValue;
                else
                    tile.ValuesHere.Add(LayersEnum.Elevation, noiseValue);

                
                heightmap[i, j] = noiseValue;
                final[i, j] += noiseValue; //Add the layers values to the final heightmap array
                Tile finalTile = finalMap.Tiles[i, j];
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

    public void UpdateTerrainRegion(int xBase, int yBase, float[,] heightmap)
    {
        terrain.terrainData.SetHeights(xBase, yBase, heightmap);
    }

}