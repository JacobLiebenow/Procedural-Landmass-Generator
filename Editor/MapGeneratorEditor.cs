using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

//Simply put, this class is designed to create a button to generate the map within the editor for the scene view.
//Creates an auto-update checkbox that automatically updates the map when any value is changed in real time
//Otherwise, the map will update in the scene view once the new "Generate" button is hit.
//These options only appear for the MapGenerator script's component
[CustomEditor (typeof (MapGenerator))]
public class MapGeneratorEditor : Editor {

    public override void OnInspectorGUI()
    {
        MapGenerator mapGen = (MapGenerator)target;

        if (DrawDefaultInspector())
        {
            if(mapGen.autoUpdate)
            {
                mapGen.DrawMapInEditor();
            }
        }

        if(GUILayout.Button ("Generate"))
        {
            mapGen.DrawMapInEditor();
        }
    }
}
