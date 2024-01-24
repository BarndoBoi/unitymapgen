using UnityEngine;
using System.Collections;
using ProcGenTiles;

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

    private Map map;
    [SerializeField]
    private FastNoiseLite noise;
    [SerializeField]
    private FastNoiseLiteParams noiseParams = new FastNoiseLiteParams();

    public void Start()
    {
        map = new Map(width, height);
        ReadNoiseParams(); //Init the fastnoise class with the serialized struct values
        GenerateHeightmap(); //Seed the tiles with a set of elevation noise
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

    public void GenerateHeightmap()
    {
        if (map.Width != width || map.Height != height)
        {
            //The map sizes have changed and we should regenerate the backing map sizes
            map = new Map(width, height);
        }
        for (int i = 0; i < height; i++)
        {
            for (int j = 0; j < width; j++)
            { //Inner for loop does most of the heavy lifting
                Tile t = map.Tiles[i, j]; //Get the tile at the location

            }
        }
    }

}