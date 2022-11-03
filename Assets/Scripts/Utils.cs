using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Linq;

public class Utils
{
	public static List<Mover> movers = new List<Mover>();

	// These are built into newer versions of Unity.
	public static Vector3Int forward { get { return new Vector3Int(0, 0, 1); } }
	public static Vector3Int back { get { return new Vector3Int(0, 0, -1); } }

	public static IEnumerator LoadScene(string scene) {
		yield return WaitFor.EndOfFrame;
		SceneManager.LoadScene(scene, LoadSceneMode.Single);
	}

	private static List<string> allLevelsRef;
	public static List<string> allLevels {
		get {
			if (allLevelsRef == null) {
				allLevelsRef = new List<string>();
				Object[] levels = Resources.LoadAll("Levels");
				foreach (Object t in levels) {
					allLevelsRef.Add(t.name);
				}
			}
			return allLevelsRef;
		}
	}
	public static void RefreshLevels() {
		allLevelsRef = null;
	}

	public static string sceneName => SceneManager.GetActiveScene().name;
	public static bool isMetaScene => LevelManager.currentLevelName.Contains("0_");

    public static int StringToInt(string intString) {
        int i = 0;
        if (!System.Int32.TryParse(intString, out i)) {
            i = 0;
        }
        return i;
    }

	public static bool Roughly(float one, float two, float tolerance = 0.5f) {
		return Mathf.Abs(one - two) < tolerance;
    }

    public static bool VectorRoughly(Vector3 one, Vector3 two, float t = 0.5f) {
        return Roughly(one.x, two.x, t) && Roughly(one.y, two.y, t) && Roughly(one.z, two.z, t);
    }

	public static bool VectorRoughly2D(Vector3 one, Vector3 two, float t = 0.5f) {
        return Roughly(one.x, two.x, t) && Roughly(one.y, two.y, t);
    }

    public static void RoundPosition(Transform t) {
    	Vector3 p = t.position;
    	t.position = Vec3ToInt(p);
    }

    public static void RoundRotation(Transform t) {
		Vector3 r = t.eulerAngles;
		r = StandardizeRotation(r);
		t.eulerAngles = Vec3ToInt(r);
    }

    public static Vector3 StandardizeRotation(Vector3 v) {
       
        if (v.z < -5) {
            float z = v.z;
            while (z < 0) {
                z += 360;
            }
            return new Vector3(v.x, v.y, z);

        } else if (v.z > 355) {
            float z = v.z;
            while (z > 355) {
                z -= 360;
            }
            return new Vector3(v.x, v.y, z);

        } else {
            return v;
        }
    }

    public static void AvoidIntersect(Transform root) {
		bool intersecting = true;
		while (intersecting) {
			intersecting = false;
			foreach (Transform tile in root) {
				if (tile.gameObject.CompareTag("Tile")) {
					Mover m = GetMoverAtPos(tile.position);
					if (m != null && m.transform != root) {
						root.position += Vector3.back;
						intersecting = true;
					} else {
						Wall wall = GetWallAtPos(tile.position);
						if (wall != null && wall.transform != root) {
							root.position += Vector3.back;
							intersecting = true;
						}
					}
				}
			}
		}
    }

	public static Vector3 AvoidIntersect(Vector3 v) {
    	bool intersecting = true;
    	while (intersecting) {
    		intersecting = false;

			if (!TileIsEmpty(v)) {
				v += Vector3.back;
				intersecting = true;
			}
    	}
		return v;
	}

	public static Vector3Int Vec3ToInt(Vector3 v) {
		return Vector3Int.RoundToInt(v);
	}

	public static bool TileIsEmpty(Vector3 pos, bool ignorePlayer) {
		return TileIsEmpty(Vec3ToInt(pos), ignorePlayer);
	}

	public static bool TileIsEmpty(Vector3Int pos, bool ignorePlayer) {
		if (WallIsAtPos(pos)) return false;
		Mover m = GetMoverAtPos(pos);
		if (m != null && !m.isPlayer) return false;
		return true;
	}

	public static bool TileIsEmpty(Vector3 pos) {
		return TileIsEmpty(Vec3ToInt(pos));
	}

	public static bool TileIsEmpty(Vector3Int pos) {
		return WallIsAtPos(pos) == false && MoverIsAtPos(pos) == false; 
	}

	private static HashSet<GameObject> GetTilesAt(Vector3Int pos)
	{
		if (Game.instance == null) return new HashSet<GameObject>();
		return Game.instance.Grid.GetContentsAt(pos);
	}

	private static T GetObjAtPos<T>(Vector3Int pos)
	{
		foreach (var tile in GetTilesAt(pos)) {
			var o = tile.GetComponentInParent<T>();
			if (o != null) { 
				return o;
			}
		}

		return default;
	}

	public static GameObject GetTaggedObjAtPos(Vector3Int pos, string tag)
	{
		foreach (var tile in GetTilesAt(pos)) {
			if (tile.transform.parent.CompareTag(tag)) {
				return tile;
			}
		}
		return null;
	}

	public static bool TaggedObjIsAtPos(Vector3Int pos, string tag)
	{
		return GetTaggedObjAtPos(pos, tag) != null;
	}

	// WALLS // 

	public static Wall GetWallAtPos(Vector3Int pos)
	{
		return GetObjAtPos<Wall>(pos);
	}

	public static Wall GetWallAtPos(Vector3 pos) {
		return GetWallAtPos(Vec3ToInt(pos));
	}

	public static bool WallIsAtPos(Vector3Int pos) {
		return GetWallAtPos(pos) != null;
	}

	// MOVERS // 

	public static Mover GetMoverAtPos(Vector3 pos) {
		return GetMoverAtPos(Vec3ToInt(pos));
	}

	public static Mover GetMoverAtPos(Vector3Int pos) {
		return GetObjAtPos<Mover>(pos);
	}

	public static bool MoverIsAtPos(Vector3 pos) {
		return MoverIsAtPos(Vec3ToInt(pos));
	}

	public static bool MoverIsAtPos(Vector3Int pos) {
		return GetMoverAtPos(pos) != null;
	}

	public static List<Mover> MoversAbove(Mover m, bool clear = true) {
		if (clear) {
			movers.Clear();
		}
		foreach (Tile t in m.tiles) {
			Mover m2 = GetMoverAtPos(t.pos + Vector3.back);
			if (m2 != null && !movers.Contains(m2)) {
				movers.Add(m2);
				movers.AddRange(MoversAbove(m2, false));
			}
		}
		return movers.Distinct().ToList();
	}

	public static bool AIsHigherThanB(Transform a, Transform b) {
		return a.position.z < b.position.z;
	}

	public static bool GroundBelowPosition(Vector3Int v, Mover source = null) {
		Vector3Int posToCheck = v + forward;
		if (WallIsAtPos(posToCheck)) {
			return true;
		}
		Mover m = GetMoverAtPos(posToCheck);
		if (m != null && m != source && !m.isFalling) {
			return true;
		}
		return false;
	}

	public static bool GroundBelowTile(Tile tile) {
		return GroundBelowPosition(tile.pos);
	}

	public static bool GroundBelowPlayer() {
		return GroundBelow(Player.instance);
	}

	public static bool GroundBelow(Mover m) {
		foreach (Tile tile in m.tiles)
		{
			if (tile.pos.z == 0)
				return true;
			if (Utils.GroundBelowTile(tile)) {
				return true;
			}
		}
		return false;
	}

	public static bool PlayerAtPos(Vector3 v) {
		Mover m = GetMoverAtPos(v);
		if (m != null && m.isPlayer) return true;
		return false;
	}
	 
	public static Texture2D MakeTex(int width, int height, Color col) {
        Color[] pix = new Color[width*height];
 
        for(int i = 0; i < pix.Length; i++)
            pix[i] = col;
 
        Texture2D result = new Texture2D(width, height);
        result.SetPixels(pix);
        result.Apply();
        return result;
    }
}
