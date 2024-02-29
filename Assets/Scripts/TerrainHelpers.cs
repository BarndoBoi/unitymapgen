using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainHelpers : MonoBehaviour
{

    [SerializeField]
    LayerTerrain terrain;

    public void GenerateNewSeed()
    {
        foreach (MapLayers layers in terrain.layersDict.Values)
        {
            foreach (MapNoisePair pair in layers.NoisePairs)
            {
                pair.NoiseParams.seed = Random.Range(int.MinValue, int.MaxValue);         
            }
        }
        terrain.GenerateTerrain();
    }

}
