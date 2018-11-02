using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Noise {

    public enum NormalizeMode {Local, Global};

    //The purpose of this method is to create a noise map based off width, height, scale, octaves, persistance, and lacunarity.
    //Octaves are, essentially, how mnany times the map will run through the system, with each octave increasing in frequency according to its square value
    //
    //(In signal processing, this is literally the case - yay, my electrical engineering degree is useful here!)
    //
    //The persistance value is the degree to which higher octaves will affect the overall height map
    //The lacunarity affects where the octaves will be placed according to given frequency
    //
    //The seed also allows for the system to recall the map based off the given seed value
    //An offset is given to more directly affect the sampling within the map as opposed to just the seed
    public static float[,] GenerateNoiseMap (int mapWidth, int mapHeight, int seed, float scale, int octaves, float persistance, float lacunarity, Vector2 offset, NormalizeMode normalizeMode)
    {
        float[,] noiseMap = new float[mapWidth, mapHeight];

        System.Random prng = new System.Random(seed);
        Vector2[] octaveOffsets = new Vector2[octaves];

        float maxPossibleHeight = 0;
        float amplitude = 1;
        float frequency = 1;

        //The seed will offset the sample values according to the RNG given for the seed, providing the octave with an appropriate value to be added or subtracted
        for (int i = 0; i < octaves; i++)
        {
            //The perlin noise generator seems to work best when random values are set between -100,000 and 100,000
            float offsetX = prng.Next(-100000, 100000) + offset.x;
            float offsetY = prng.Next(-100000, 100000) - offset.y;

            octaveOffsets[i] = new Vector2(offsetX, offsetY);

            maxPossibleHeight += amplitude;
            amplitude *= persistance;
        }

        //A negative scale or a scale equal to zero are simply not mathematically possible here, so if those are given, set it equal to some epsilon value
        if (scale <= 0)
        {
            scale = 0.0001f;
        }

        //Create max and min values to normalize the noise map to [0->1] once it's finished
        float maxLocalNoiseHeight = float.MinValue;
        float minLocalNoiseHeight = float.MaxValue;

        //Force the scale to zoom in and out relative to the center of the noise map as opposed to the top right by subtracting the half-values from x and y
        float halfWidth = mapWidth / 2f;
        float halfHeight = mapHeight / 2f;

        //The noise map itself will be generated based off width, height, and octaves.  Amplitude scales with persistance, frequency scales with lacunarity
        //Each point is sampled from the perlin noise generator
        //Perlin noise is used to generate the amplitude value within the given signals for each octave.  Negative values are allowed here to create a more interesting map
        for (int y = 0; y < mapHeight; y++)
        {
            for (int x = 0; x < mapWidth; x++)
            {
                amplitude = 1;
                frequency = 1;
                float noiseHeight = 0;
                for (int i = 0; i < octaves; i++)
                {
                    float sampleX = (x- halfWidth + octaveOffsets[i].x) / scale * frequency;
                    float sampleY = (y- halfHeight + octaveOffsets[i].y) / scale * frequency;
                    float perlinValue = Mathf.PerlinNoise(sampleX, sampleY) * 2 - 1;
                    noiseHeight += perlinValue * amplitude;
                    amplitude *= persistance;
                    frequency *= lacunarity;
                }

                //Define the maximum and minimum height values as they're generated
                if (noiseHeight > maxLocalNoiseHeight)
                {
                    maxLocalNoiseHeight = noiseHeight;
                } else if (noiseHeight < minLocalNoiseHeight) {
                    minLocalNoiseHeight = noiseHeight;
                }
                noiseMap[x, y] = noiseHeight;
            }
        }

        //Normalize the noise map for each value within the 2D array using an inverse lerp
        for (int y = 0; y < mapHeight; y++)
        {
            for (int x = 0; x < mapWidth; x++)
            {
                if (normalizeMode == NormalizeMode.Local)
                {
                    noiseMap[x, y] = Mathf.InverseLerp(minLocalNoiseHeight, maxLocalNoiseHeight, noiseMap[x, y]);
                } else
                {
                    float normalizedHeight = (noiseMap[x, y] + 1) / (maxPossibleHeight);
                    noiseMap[x, y] = Mathf.Clamp(normalizedHeight, 0, int.MaxValue);
                }
            }
        }

                return noiseMap;
    }
	
}
