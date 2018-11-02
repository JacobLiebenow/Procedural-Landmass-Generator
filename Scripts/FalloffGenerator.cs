using System.Collections;
using System.Collections.Generic;
using UnityEngine;


//This class will generate a falloff map that will be subtracted from a locally-generated map, creating, in effect, an island terrain
public static class FalloffGenerator {

	public static float[,] GenerateFalloffMap(int size)
    {
        float[,] map = new float[size, size];

        for (int i = 0; i < size; i++)
        {
            for (int j = 0; j < size; j++)
            {
                float x = (i / (float)size * 2) - 1;
                float y = (j / (float)size * 2) - 1;

                float value = Mathf.Max(Mathf.Abs(x), Mathf.Abs(y));
                map[i, j] = Evaluate(value);
            }
        }

        return map;
    }

    //Using this method allows for the falloff map to be generated along a curve of values as opposed to a linear pattern
    static float Evaluate(float value)
    {
        float a = 3;
        float b = 2.2f;

        return Mathf.Pow(value, a) / (Mathf.Pow(value, a) + Mathf.Pow(b - b * value, a));
    }
}
