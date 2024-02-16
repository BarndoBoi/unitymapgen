using UnityEngine;

[System.Serializable]
// explains the concepts: frequency, wavelength, amplitude, octaves, pink and blue and white noise, etc. (low level)
// https://www.redblobgames.com/articles/noise/introduction.html

// main article 
// https://www.redblobgames.com/maps/terrain-from-noise/
public class NoiseParams
{
    public FastNoiseLite.NoiseType noiseType = FastNoiseLite.NoiseType.Perlin;

    // lower frequencies make wider hills and higher frequencies make narrower hills.
    public float frequency = 0.04f;

    // Seed for the map
    public int seed = 10;

    public FastNoiseLite.FractalType fractalType = FastNoiseLite.FractalType.PingPong;

    // The lacunarity specifies the frequency multipler between successive octaves.
    // The effect of modifying the lacunarity is subtle;
    // You may need to play with the lacunarity value to determine the effects.
    // For best results, set the lacunarity to a number between 1.5 and 3.5.
    public float fractalLacunarity = 2;

    // the ratio of increase for each fractal octave.
    // [1, 1/2, 1/4, 1/8, 1/16, …],
    // where each amplitude is 1/2 the previous one. 
    //
    // Don't have to use a fixed ratio,
    // could do [1, 1/2, 1/3, 1/4, 1/5] 
    // to get finer details than what the conventional amplitudes allow
    // but need to check that FNL supports that sorta thing
    // thank god it's open source :3
    public float fractalGain = 0.5f;
    public float weightedStrength = 1;
    public int fractalOctaves = 4; // how many different frequencies we are using, each being distanced by the fractalGain

    // Non-FastNoiseLite values
    //
    // This is the power for the noise to be raised to, in order to get flat valleys.
    // Set to 1 for no change.
    public float raisedPower = 3f;

    //Minimum height value so that we can set into ground to get flat ocean.
    public float minValue = .5f;

    public string SerializeParamsToJson()
    {
        return JsonUtility.ToJson(this, true); //Always export with prettyprint
    }
}
