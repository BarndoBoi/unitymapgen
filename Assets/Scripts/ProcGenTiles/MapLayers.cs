using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProcGenTiles
{
    public class MapLayers
    {
        public int Width, Height;
        public Dictionary<Map, FastNoiseLiteParams> Layers = new Dictionary<Map, FastNoiseLiteParams>();

        public MapLayers(int width, int height)
        {
            Width = width;
            Height = height;
        }

        public void AddLayer(Map map, FastNoiseLiteParams noiseParams)
        {
            Layers.Add(map, noiseParams); //Link the map and the noiseParams
        }

        public void AddNewMapLayer(FastNoiseLiteParams noiseParams)
        {
            Layers.Add(new Map(Width, Height), noiseParams);
        }

        public float ReturnSumOfNoiseLayers()
        {
            //Noise ranges from 0-1, so add one per layer of the map
            //Subject to change later so don't get attached to this
            return Layers.Count;
        }
    }
}
