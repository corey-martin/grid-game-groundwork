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

	public static bool TileIsEmpty(Vector3 pos) {
		return TileIsEmpty(Vec3ToInt(pos));
	}

	public static bool TileIsEmpty(Vector3Int pos) {
		return WallIsAtPos(pos) == false && MoverIsAtPos(pos) == false; 
	}

	private static HashSet<GameObject> GetTilesAt(Vector3Int pos)
	{
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
		foreach (var tile in GetTilesAt(pos))
			if (tile.CompareTag(tag))
				return tile;
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
}
