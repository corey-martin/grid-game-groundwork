using System.Collections.Generic;
using UnityEngine;

public class LevelLoader
{
    static int levelIndex = 0;
    static List<string> allLevels => Utils.allLevels;

    public static SerializedLevel LoadLevel(string levelName) {
        SetLevelIndexByName(levelName);
        TextAsset textFile = Resources.Load<TextAsset>("Levels/" + levelName);
        return JsonUtility.FromJson<SerializedLevel>(textFile.text);
    }

    public static void InstantiateLevel(SerializedLevel level, GameObject[] prefabs, Transform parent) {
        foreach (var slo in level.LevelObjects) {
            foreach (GameObject prefab in prefabs) {
                if (prefab.transform.name == slo.prefab) {
                    var go = GameObject.Instantiate(prefab) as GameObject;
                    go.transform.parent = parent;
                    go.transform.localPosition = slo.pos;
                    go.transform.localEulerAngles = slo.angles;
                }
            }
        }
    }
    
    public static SerializedLevel LoadNextLevel() {
        levelIndex++; 
        if (levelIndex >= allLevels.Count) {
            levelIndex = 0;
        }
        if (allLevels[levelIndex].Contains("test")) {
            return LoadNextLevel();
        } else {
            return LoadLevel(allLevels[levelIndex]);
        }
    }

    static void SetLevelIndexByName(string levelName) {
        for (int i = 0; i < allLevels.Count; i++) {
            if (allLevels[i] == levelName) {
                levelIndex = i;
            }
        }
    }
}
