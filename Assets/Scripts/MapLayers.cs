using System.Collections.Generic;
using ProcGenTiles;
using UnityEngine;

[System.Serializable]
public class MapLayers
{
    int Width, Height;
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
        //Noise ranges from -1 to 1 on each noiseParam, which is then multiplied by a power
        //Noise is additive with other layers, so maximum noise range is all noise ranges summed together
        float sum = 0;
        foreach(var pair in NoisePairs)
        {
            sum += Mathf.Pow(1, pair.NoiseParams.raisedPower);
        }
        return sum; //This is the highest sum that could be added together for this map
    }
}
