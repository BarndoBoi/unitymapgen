using System.Collections.Generic;
using ProcGenTiles;

[System.Serializable]
public class MapLayers
{
    public int Width, Height;
    public List<MapNoisePair> NoisePairs = new List<MapNoisePair>();

    public MapLayers(int width, int height)
    {
        Width = width;
        Height = height;
    }

    public void AddLayer(Map map, NoiseParams noiseParams)
    {
        NoisePairs.Add(new MapNoisePair(map, noiseParams)); //Link the map and the noiseParams
    }

    public void AddLayer(MapNoisePair noisePair)
    {
        NoisePairs.Add(noisePair);
    }

    public void AddNewMapLayer(NoiseParams noiseParams)
    {
        NoisePairs.Add(new MapNoisePair(new Map(Width, Height), noiseParams));
    }

    public float SumOfNoiseLayers()
    {
        //Noise ranges from 0-1, so add one per layer of the map
        //Subject to change later so don't get attached to this
        return NoisePairs.Count;
    }
}
