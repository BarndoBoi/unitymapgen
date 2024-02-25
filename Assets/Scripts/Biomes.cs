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
    }
       
    public List<IndexValue> AllBiomes = new List<IndexValue>();
}
