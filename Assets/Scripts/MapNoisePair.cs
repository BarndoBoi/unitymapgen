using ProcGenTiles;
using UnityEngine;

[System.Serializable]
public class MapNoisePair
{
    public Map Map;
    public NoiseParams NoiseParams;
    public TextAsset JSON;
    public bool UseJsonFile;

    public MapNoisePair(Map map, NoiseParams noiseParams)
    {
        this.Map = map;
        this.NoiseParams = noiseParams;
    }
}
