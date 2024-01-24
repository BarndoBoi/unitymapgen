using UnityEngine;
using System.Collections;
using ProcGenTiles;
using System.Linq;

public class TerrainFromTilemap : MonoBehaviour
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

    private Map map;
    [SerializeField]
    private FastNoiseLite noise;
    [SerializeField]
    private FastNoiseLiteParams noiseParams = new FastNoiseLiteParams();
    [SerializeField]
    private Terrain terrain;

    public void Start()
    {
        map = new Map(width, height);
        if (terrain == null)
            terrain = GetComponent<Terrain>(); //Should already be assigned, but nab it otherwise
        GenerateTerrain();
    }

    public void GenerateTerrain()
    {
        ReadNoiseParams(); //Init the fastnoise class with the serialized struct values
        float[,] heights = GenerateHeightmap(); //Seed the tiles with a set of elevation noise
        CreateTerrainFromHeightmap(heights, terrain.terrainData);
    }

    public void CreateTerrainFromHeightmap(float[,] heights, TerrainData terrainData)
    {
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

                float height = terrainData.GetHeight(Mathf.RoundToInt(y_01 * terrainData.heightmapResolution),
                    Mathf.RoundToInt(x_01 * terrainData.heightmapResolution));
                Vector3 normal = terrainData.GetInterpolatedNormal(y_01, x_01);
                float steepness = terrainData.GetSteepness(y_01, x_01);

                // Setup an array to record the mix of texture weights at this point
                float[] splatWeights = new float[terrainData.alphamapLayers];

                splatWeights[0] = 0.0f;
                splatWeights[1] = 0.0f;
                splatWeights[2] = 0.0f;
                splatWeights[3] = 0.0f;

                // the percent of terrains max height that this area is
                float hm_perc = (height / terrainData.heightmapResolution) * 10f;

                Biome(); //sets the biome

                void Biome()
                {
                    // will need to tune further but this will work for the basics now. 
                    // RedBlob had a better implementation of this that might be worth looking into
                    if (hm_perc < 0.02) { splatWeights[2] = 1.0f; return; } //water
                    if (hm_perc < 0.10) { splatWeights[0] = 1.0f; return; } //beach sand
                    if (hm_perc < 0.45) { splatWeights[1] = 1.0f; return; } // grass
                    if (hm_perc >= 0.45) { splatWeights[3] = 1.0f; return; } //snow

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
        terrainData.SetAlphamaps(0, 0, splatmapData);
    }

    public void ReadNoiseParams()
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
    }

    public float[,] GenerateHeightmap()
    {
        if (map.Width != width || map.Height != height)
        {
            //The map sizes have changed and we should regenerate the backing map sizes
            map = new Map(width, height);
        }
        float[,] heightmap = new float[width, height];
        for (int i = 0; i < height; i++)
        {
            for (int j = 0; j < width; j++)
            { //Inner for loop does most of the heavy lifting
                Tile t = map.Tiles[i, j]; //Get the tile at the location
                float v = noise.GetNoise(i * noiseScale, j * noiseScale); //Grab the value
                v = Mathf.Lerp(-1, 1, v); //Normalize the returned noise
                //Set the elevation to the normalized value by checking if we've already set elevation data
                if (t.ValuesHere.ContainsKey(LayersEnum.Elevation))
                    t.ValuesHere[LayersEnum.Elevation] = v;
                else
                    t.ValuesHere.Add(LayersEnum.Elevation, v);

                heightmap[i, j] = v; //Place in 2d float array as well.
            }
        }
        return heightmap;
    }

}