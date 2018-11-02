using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//CreateAssetMenu allows for the creation of objects that has the ability to define and save public variables and other information
[CreateAssetMenu()]
public class NoiseData : UpdatableData {

    public Noise.NormalizeMode normalizeMode;

    public float scale;

    public int octaves;
    [Range(0, 1)]
    public float persistance;
    public float lacunarity;

    public int seed;
    public Vector2 offset;

    //This is called when a value is changed within the inspector
    //A less well-known method, but good for clamping values and keeping the system from crashing
    //
    //Previously, the map's width and heigh was be clamped in addition to the lacunarity and octaves, however mapWidth and mapHeight were both replaced by chunk size for LOD purposes
    protected override void OnValidate()
    {
        if (lacunarity < 1)
        {
            lacunarity = 1;
        }

        if (octaves < 0)
        {
            octaves = 0;
        }

        base.OnValidate();
    }
}
