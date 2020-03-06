using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Utils
{
	public static IEnumerator LoadScene(string scene) {
		yield return WaitFor.EndOfFrame;
		SceneManager.LoadScene(scene, LoadSceneMode.Single);
	}

    public static int StringToInt(string intString) {
        int i = 0;
        if (!System.Int32.TryParse(intString, out i)) {
            i = 0;
        }
        return i;
    }

    public static void AvoidIntersect(Transform t) {
		foreach (Transform child in t) {
			if (child.gameObject.CompareTag("Tile")) {
				SubAvoidIntersect(child, t);
			}
		}
    }

    public static void SubAvoidIntersect(Transform tile, Transform root) {
    	bool intersecting = true;
    	while (intersecting) {
    		intersecting = false;
			Mover m = GetMoverAtPos(tile.position);
			if (m != null && m.transform != root) {
				root.position += new Vector3(0,0,-1);
				intersecting = true;
			} else {
				Wall wall = GetWallAtPos(tile.position);
				if (wall != null && wall.transform != root) {
					root.position += new Vector3(0,0,-1);
					intersecting = true;
				}
			}
    	}
    }

	public static Vector3Int Vec3ToInt(Vector3 v) {
		return Vector3Int.RoundToInt(v);
	}

	// WALLS // 

	public static Wall GetWallAtPos(Vector3Int pos) {
		if (Game.wallDict.ContainsKey(pos)) {
			Wall wall = Game.wallDict[pos];
			return wall;
		}
		return null;
	}

	public static Wall GetWallAtPos(Vector3 pos) {
		return GetWallAtPos(Vec3ToInt(pos));
	}

	public static bool WallIsAtPos(Vector3 pos) {
		return WallIsAtPos(Vec3ToInt(pos));
	}

	public static bool WallIsAtPos(Vector3Int pos) {
		return GetWallAtPos(pos) != null;
	}

	// MOVERS // 

	public static Mover GetMoverAtPos(Vector3 pos) {
		return GetMoverAtPos(Vec3ToInt(pos));
	}

	public static Mover GetMoverAtPos(Vector3Int pos) {
		if (Game.moveDict.ContainsKey(pos)) {
			Mover m = Game.moveDict[pos];
			return m;
		}
		return null;		
	}
}
