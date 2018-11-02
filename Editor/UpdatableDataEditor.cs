using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

//This simply creates a bool button in the editor to determine whether or not the terrain mesh will auto-update when a value in the data classes are changed
//The true is added to allow for classes that inherit UpdatableData to be able to use the editor, as well
[CustomEditor(typeof(UpdatableData), true)]
public class UpdatableDataEditor : Editor {

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        UpdatableData data = (UpdatableData)target;

        if(GUILayout.Button("Update"))
        {
            data.NotifyOfUpdatedValues();
            EditorUtility.SetDirty(target);
        }
    }
}
