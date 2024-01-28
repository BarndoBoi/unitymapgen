using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class FastNoiseLiteParams
{
    public FastNoiseLite.NoiseType noiseType = FastNoiseLite.NoiseType.Perlin;
    public float frequency = 0.015f;
    public int seed = 0;
    public FastNoiseLite.FractalType fractalType = FastNoiseLite.FractalType.Ridged;
    public float fractalLacunarity = 2;
    public float fractalGain = 0.5f;
    public float weightedStrength = 0;
    public int fractalOctaves = 3;
}
