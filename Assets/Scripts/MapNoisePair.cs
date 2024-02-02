using System.Collections;
using ProcGenTiles;

[System.Serializable]
public class MapNoisePair
{
    public Map Map;
    public NoiseParams NoiseParams;

    public MapNoisePair(Map map, NoiseParams noiseParams)
    {
        this.Map = map;
        this.NoiseParams = noiseParams;
    }
}
