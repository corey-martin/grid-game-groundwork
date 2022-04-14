using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[System.Serializable]
public class SerializedLevel
{
    public List<SerializedLevelObject> LevelObjects;

    public SerializedLevel(GameObject level)
    {
        LevelObjects = new List<SerializedLevelObject>();
        foreach (Transform child in level.transform)
        {
            LevelObjects.Add(new SerializedLevelObject(child));
        }
    }
}

[System.Serializable]
public class SerializedLevelObject
{
    public string prefab;
    public Vector3 pos;
    public Vector3 angles;
    public SerializedLevelObject(Transform t)
    {
        prefab = t.name;
        pos = t.localPosition;
        angles = t.localEulerAngles;
    }
}