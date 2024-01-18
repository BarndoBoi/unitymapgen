using UnityEngine;



public class TerrainGenerator : MonoBehaviour
{
    public int width = 256; //x-axis of the terrain
    public int height = 256; //z-axis

    public int depth = 20; //y-axis

    public float scale = 35f;

    public float offsetX = 100f;
    public float offsetY = 100f;

    //offsetX = Random.Range(0f, 9999f);
    //offsetY = Random.Range(0f, 9999f);

    //Init map and prep noise for terrain layer

    float noiseScale = 0.9f;
    float noiseFrequency = 0.25f;
    int seed = 10;

    FastNoiseLite noise = new FastNoiseLite();

    // gives us a random noise each time
    private void Start()
    {
        offsetX = Random.Range(0f, 9999f);
        offsetY = Random.Range(0f, 9999f);
    }
    // Using Update() instead of Start() for testing
    // can update scale in real-time

    private void Update()
    {

        

        Terrain terrain = GetComponent<Terrain>(); //so we can interact with the Terrain object
        terrain.terrainData = GenerateTerrain(terrain.terrainData);
    }

    TerrainData GenerateTerrain(TerrainData terrainData)
    {
        // sets initial Terrain data
        terrainData.heightmapResolution = width + 1;
        terrainData.size = new Vector3(width, depth, height);

        //sets heights with perlin
        terrainData.SetHeights(0, 0, GenerateHeights());
        return terrainData;
    }

    float[,] GenerateHeights()
    {
        float[,] heights = new float[width, height];
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                heights[x, y] = CalculateHeight(x, y);
            }
        }

        return heights;
    }

    float CalculateHeight(int x, int y)
    {
        float xCoord = (float)x / width * scale + offsetX;
        float yCoord = (float)y / height * scale + offsetY;

        noise.SetNoiseType(FastNoiseLite.NoiseType.Perlin);
        noise.SetFrequency(noiseFrequency);
        noise.SetSeed(seed);

        float value = noise.GetNoise(xCoord * noiseScale, yCoord * noiseScale);

        return value;
    }
}
