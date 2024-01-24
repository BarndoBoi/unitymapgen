using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct FastNoiseLiteParams
{
    public FastNoiseLite.NoiseType noiseType;
    public float frequency;
    public int seed;
    public FastNoiseLite.FractalType fractalType;
    public float fractalLacunarity;
    public float fractalGain;
    public int fractalOctaves;
}
