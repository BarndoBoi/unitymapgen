using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Biomes : MonoBehaviour
{
    [System.Serializable]
   public class IndexValue
    {
        public string name;
        public float value;
        public int index;
        public bool isWaterLayer; //Marks if this value should be used in land/water checking
    }
       
    public List<IndexValue> AllBiomes = new List<IndexValue>();

    public IndexValue GetWaterLayer()
    {
        for (int i = 0; i < AllBiomes.Count; i++)
        {
            if (AllBiomes[i].isWaterLayer)
            {
                return AllBiomes[i];
            }
        }
        return null; //Also need to throw an error
    }
}
