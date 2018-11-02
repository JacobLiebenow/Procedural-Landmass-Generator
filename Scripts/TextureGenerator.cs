using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//This class is mostly used to organize the types of texture generation methods as defined in previous classes
public static class TextureGenerator {
    
    //This method will generate a texture according to the color map passed in, the width, and the height
    public static Texture2D TextureFromColorMap (Color[] colorMap, int width, int height)
    {
        Texture2D texture = new Texture2D(width, height);
        texture.filterMode = FilterMode.Point;
        texture.wrapMode = TextureWrapMode.Clamp;
        texture.SetPixels(colorMap);
        texture.Apply();
        return texture;
    }

    //This specific method utilizes the TextureFromColorMap method, however it uses a generic white-to-black map based off height alone
    public static Texture2D TextureFromHeightMap(float[,] heightMap)
    {
        int width = heightMap.GetLength(0);
        int height = heightMap.GetLength(1);

        Color[] colorMap = new Color[width * height];
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                colorMap[y * width + x] = Color.Lerp(Color.black, Color.white, heightMap[x, y]);
            }
        }

        return TextureFromColorMap(colorMap, width, height);
    }
	
}
