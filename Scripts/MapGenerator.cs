using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;


public class MapGenerator : MonoBehaviour {

    //Determine what type of map will be drawn - something strictly based off noise, or based off the region colors
    public enum DrawMode {NoiseMap/*, ColorMap*/, Mesh, FalloffMap};
    public DrawMode drawMode;

    public TerrainData terrainData;
    public NoiseData noiseData;
    public TextureData textureData;

    public Material terrainMaterial;

    [Range(0,6)]
    public int editorPreviewLOD;
    
    public bool autoUpdate;

    //public TerrainType[] regions;

    float[,] falloffMap;

    Queue<MapThreadInfo<MapData>> mapDataThreadInfoQueue = new Queue<MapThreadInfo<MapData>>();
    Queue<MapThreadInfo<MeshData>> meshDataThreadInfoQueue = new Queue<MapThreadInfo<MeshData>>();

    void OnValuesUpdated()
    {
        if(!Application.isPlaying)
        {
            DrawMapInEditor();
        }
    }

    void OnTextureValuesUpdated()
    {
        textureData.ApplyToMaterial(terrainMaterial);
    }

    public int mapChunkSize
    {
        get {
            if (terrainData.useFlatshading)
            {
                return 95;
            } else
            {
                return 239;
            }
        }
    }

    //Create the map as outlined within the Noise class according to the public variables defined within the Unity editor
    public void DrawMapInEditor()
    {
        textureData.UpdateMeshHeights(terrainMaterial, terrainData.minHeight, terrainData.maxHeight);
        MapData mapData = GenerateMapData(Vector2.zero);
        MapDisplay display = FindObjectOfType<MapDisplay>();
        if (drawMode == DrawMode.NoiseMap)
        {
            display.DrawTexture(TextureGenerator.TextureFromHeightMap(mapData.heightMap));
        }
        /*else if (drawMode == DrawMode.ColorMap)
        {
            display.DrawTexture(TextureGenerator.TextureFromColorMap(mapData.colorMap, mapChunkSize, mapChunkSize));
        }*/
        else if (drawMode == DrawMode.Mesh)
        {
            display.DrawMesh(MeshGenerator.GenerateTerrainMesh(mapData.heightMap, terrainData.meshHeightMultiplier, terrainData.meshHeightCurve, editorPreviewLOD, terrainData.useFlatshading)/*, TextureGenerator.TextureFromColorMap(mapData.colorMap, mapChunkSize, mapChunkSize)*/);
        } else if (drawMode == DrawMode.FalloffMap)
        {
            display.DrawTexture(TextureGenerator.TextureFromHeightMap(FalloffGenerator.GenerateFalloffMap(mapChunkSize)));
        }
    }

    //During the map generation, as the viewer shifts and the chunks need loading, threading is used to make sure the system resources are used more efficiently
    //The direct benefit of this is that it keeps the system from locking up or overheating
    //This will utilize a basic call-and-response system, requiring System and System.Threading
    public void RequestMapData(Vector2 center,Action<MapData> callback)
    {
        textureData.UpdateMeshHeights(terrainMaterial, terrainData.minHeight, terrainData.maxHeight);
        ThreadStart threadStart = delegate
        {
            MapDataThread(center, callback);
        };

        new Thread(threadStart).Start();
    }

    //This has a lock within it to make sure mapDataThreadInfoQueue is only used during this thread, and cannot be called elsewhere - no other thread can call it, and it will have to wait
    //Also enqueues the threading info
    void MapDataThread(Vector2 center, Action<MapData> callback)
    {
        MapData mapData = GenerateMapData(center);
        lock (mapDataThreadInfoQueue)
        {
            mapDataThreadInfoQueue.Enqueue(new MapThreadInfo<MapData>(callback, mapData));
        }
    }

    public void RequestMeshData(MapData mapData, int lod, Action<MeshData> callback)
    {
        ThreadStart threadStart = delegate {
            MeshDataThread(mapData, lod, callback);
        };
        new Thread(threadStart).Start();
    }

    void MeshDataThread(MapData mapData, int lod, Action<MeshData> callback)
    {
        MeshData meshData = MeshGenerator.GenerateTerrainMesh(mapData.heightMap, terrainData.meshHeightMultiplier, terrainData.meshHeightCurve, lod, terrainData.useFlatshading);
        lock (meshDataThreadInfoQueue)
        {
            meshDataThreadInfoQueue.Enqueue(new MapThreadInfo<MeshData>(callback, meshData));
        }
    }

    private void Update()
    {
        //If there is more than one element within the queue, loop through all elements, proceed to dequeue them, and call callback
        if (mapDataThreadInfoQueue.Count > 0)
        {
            for (int i = 0; i < mapDataThreadInfoQueue.Count; i++)
            {
                MapThreadInfo<MapData> threadInfo = mapDataThreadInfoQueue.Dequeue();
                threadInfo.callback(threadInfo.parameter);
            }
        }

        if (meshDataThreadInfoQueue.Count > 0)
        {
            for (int i = 0; i < meshDataThreadInfoQueue.Count; i++)
            {
                MapThreadInfo<MeshData> threadInfo = meshDataThreadInfoQueue.Dequeue();
                threadInfo.callback(threadInfo.parameter);
            }
        }
    }

    //Generate the MapData structure according to the variety of publically-set variables
    MapData GenerateMapData(Vector2 center)
    {
        float[,] noiseMap = Noise.GenerateNoiseMap(mapChunkSize + 2, mapChunkSize + 2, noiseData.seed, noiseData.scale, noiseData.octaves, noiseData.persistance, noiseData.lacunarity, center + noiseData.offset, noiseData.normalizeMode);

        //Each region has a specific color associated with it
        //In order to determine what each location falls under, take the current heigh as generated by the perlin noise and compare it to the regions array
        //Apply the color accordingly
        //
        //Note that to account for the border tiles in the chunks for the falloff map, the color maps must subsequently be increased in size, as well
        //However, this does create seams with differing heights elsewhere, creating tears
        //
        //In truth, I think the best fix for this would be to have one conditional that conforms to useFalloff, and another that doesn't 
        //My version of the code reflected this when the color map was still in use.
        //The seams wouldn't be an issue since the falloff map causes the height values to even out at zero on the edges anyway
        if (terrainData.useFalloff)
        {
            //Color[] colorMap = new Color[(mapChunkSize + 2) * (mapChunkSize + 2)];
            falloffMap = FalloffGenerator.GenerateFalloffMap(mapChunkSize + 2);

            for (int y = 0; y < mapChunkSize + 2; y++)
            {
                for (int x = 0; x < mapChunkSize + 2; x++)
                {
                    noiseMap[x, y] = Mathf.Clamp01(noiseMap[x, y] - falloffMap[x, y]);
                    /*float currentHeight = noiseMap[x, y];
                    for (int i = 0; i < regions.Length; i++)
                    {
                        if (currentHeight >= regions[i].height)
                        {
                            colorMap[y * mapChunkSize + x] = regions[i].color;
                        }
                        else
                        {
                            break;
                        }
                    }*/
                }
            }
        }
        /*else
        {
            //Color[] colorMap = new Color[(mapChunkSize) * (mapChunkSize)];
            for (int y = 0; y < mapChunkSize; y++)
            {
                for (int x = 0; x < mapChunkSize; x++)
                {
                    float currentHeight = noiseMap[x, y];
                    for (int i = 0; i < regions.Length; i++)
                    {
                        if (currentHeight >= regions[i].height)
                        {
                            colorMap[y * mapChunkSize + x] = regions[i].color;
                        }
                        else
                        {
                            break;
                        }
                    }
                }
            }
        }*/

        return new MapData(noiseMap/*, colorMap*/);
    }
    
    
    private void OnValidate()
    { 
        //Both terrain data and noise data need to be subscribed when they change
        //By subtracting their initial subscription, it prevents the system from calling it multiple times when updated
        if (terrainData != null)
        {
            terrainData.OnValuesUpdated -= OnValuesUpdated;
            terrainData.OnValuesUpdated += OnValuesUpdated;
        }

        if (noiseData != null)
        {
            noiseData.OnValuesUpdated -= OnValuesUpdated;
            noiseData.OnValuesUpdated += OnValuesUpdated;
        }
        
        if (textureData != null)
        {
            textureData.OnValuesUpdated -= OnTextureValuesUpdated;
            textureData.OnValuesUpdated += OnTextureValuesUpdated;
        }
    }

    //Because this will also be used for MeshData as well as MapData, this struct is to be considered generic
    struct MapThreadInfo<T>
    {
        public readonly Action<T> callback;
        public readonly T parameter;

        public MapThreadInfo(Action<T> callback, T parameter)
        {
            this.callback = callback;
            this.parameter = parameter;
        }
    }

}

//Each region can be defined by a range of values:
//  name - defines what each type of location is called
//  height - determines at what range this region will be defined at based off how high it is on the map
//  color - defines what color this region will appear as
//(If you make something serializable, it will show up in the inspector)
//
//Future iterations of this generator no longer utilize the color map generator, however I've grown fond of it
//Instead of deleting it, I simply decided to comment it out
//
/*[System.Serializable]
public struct TerrainType
{
    public string name;
    public float height;
    public Color color;
}*/

//Create a data structure for the map data that contains the height and color maps
public struct MapData
{
    public readonly float[,] heightMap;
    //public readonly Color[] colorMap;

    public MapData(float[,] heightMap/*, Color[] colorMap*/)
    {
        this.heightMap = heightMap;
        //this.colorMap = colorMap;
    }
}