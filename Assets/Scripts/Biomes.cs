using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ProcGenTiles;

public class Biomes : MonoBehaviour
{
    private LayerTerrain layerTerrain;

    private Biomes allBiomes;
    private Terrain terrain;
    public Map finalMap;
    public Dictionary<string, MapLayers> layersDict = new Dictionary<string, MapLayers>();
    public MapLayers moistureLayers;

    [System.Serializable]
   public class IndexValue
    {
        public string name;
        public float value;
        public int index;
        public bool isWaterLayer; //Marks if this value should be used in land/water checking
    }

    [System.Serializable]
    public class IndexValueList
    {
        public List<IndexValue> values = new List<IndexValue>();
    }

    public IndexValueList AllBiomes = new IndexValueList(); //This is kinda dumb, but I've got to got around the json serializer
    public TextAsset JsonFile;

    public IndexValue GetWaterLayer()
    {
        for (int i = 0; i < AllBiomes.values.Count; i++)
        {
            if (AllBiomes.values[i].isWaterLayer)
            {
                return AllBiomes.values[i];
            }
        }
        return null; //Also need to throw an error
    }

    public string SerializeToJson()
    {
        string json = JsonUtility.ToJson(this.AllBiomes, true);
        Debug.Log(json);
        return json;
    }

    // Creates new noise map for moisture\
    // this does nothing yet
    public void GenerateBiomes()
    {
        for (int i = 0; i < moistureLayers.NoisePairs.Count; i++)
        {
            MapNoisePair pair = moistureLayers.NoisePairs[i];
            if (pair.UseJsonFile)
            {
                pair.NoiseParams = JsonUtility.FromJson<NoiseParams>(pair.JSON.text);
            }
            layerTerrain.ReadNoiseParams(pair.NoiseParams); //Feed the generator this layer's info
            layerTerrain.GenerateHeightmap(pair, LayersEnum.Moisture); //This function handles adding the layer into the finalMap, but it's not very clear. Needs cleaning up to be more readable
        
        }
        layerTerrain.NormalizeFinalMap(LayersEnum.Moisture, 0, 1); //Make the final map only span from 0 to 1
    }


    public void SetBiomes()
    {

    }
}
