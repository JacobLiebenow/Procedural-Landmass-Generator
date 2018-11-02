using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Both Terrain Data and Noise Data inherit from this class
//The general idea is that, since these independently wouldn't update the terrain mesh on their own when their values change, this will simply do it for them
public class UpdatableData : ScriptableObject {

    public event System.Action OnValuesUpdated;
    public bool autoUpdate;

    //This is called because other classes that inherit UpdatableData use OnValidate() themselves
    protected virtual void OnValidate()
    {
        if(autoUpdate)
        {
            UnityEditor.EditorApplication.update += NotifyOfUpdatedValues;
        }
    }

    public void NotifyOfUpdatedValues()
    {
        UnityEditor.EditorApplication.update -= NotifyOfUpdatedValues;
        if (OnValuesUpdated != null)
        {
            OnValuesUpdated();
        }
    }

}
