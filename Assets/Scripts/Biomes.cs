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
}
