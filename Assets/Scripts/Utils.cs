using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Linq;

public class Utils
{
	public static List<Mover> movers = new List<Mover>();
	static int maxColliders = 5;

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

	// WALLS // 

	public static Wall GetWallAtPos(Vector3Int pos) {
		Collider[] colliders = new Collider[maxColliders];
        int numColliders = Physics.OverlapSphereNonAlloc(pos, 0.4f, colliders);

        for (int i = 0; i < numColliders; i++) {
			Wall wall = colliders[i].GetComponentInParent<Wall>();
			if (wall != null) {
				return wall;
			}
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
		Collider[] colliders = new Collider[maxColliders];
        int numColliders = Physics.OverlapSphereNonAlloc(pos, 0.4f, colliders);

        for (int i = 0; i < numColliders; i++) {
			Mover m = colliders[i].GetComponentInParent<Mover>();
			if (m != null) { 
				return m;
			}
        }
		return null;	
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
