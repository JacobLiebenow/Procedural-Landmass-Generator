using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapDisplay : MonoBehaviour {

    public Renderer textureRenderer;
    public MeshFilter meshFilter;
    public MeshRenderer meshRenderer;

    //For the plane that's been created within the Unity Editor, each pixel will have a certain value associated with it according to the map generated based off the noise
    //The Renderer is taken in for the plane, and then a 2D array is created to define the color map of the plane, generated a new texture for the plane's material
    //The new texture is then applied to the plane, and the shared material is changed in order to allow for the map to be seen in the "scene" view itself in the editor
    //The scale of the plane is also redefined thereafter
    //
    //The type of noise map that is used is created via the TextureGenerator class, and is defined via public variables in the MapGenerator class
    public void DrawTexture(Texture2D texture)
    {
        textureRenderer.sharedMaterial.mainTexture = texture;
        textureRenderer.transform.localScale = new Vector3(texture.width, 1, texture.height);

    }

    //This draws the mesh map according to all given info prior
    public void DrawMesh(MeshData meshData/*, Texture2D texture*/)
    {
        meshFilter.sharedMesh = meshData.CreateMesh();

        meshFilter.transform.localScale = Vector3.one * FindObjectOfType<MapGenerator>().terrainData.uniformScale;
        //meshRenderer.sharedMaterial.mainTexture = texture;
    }
}
