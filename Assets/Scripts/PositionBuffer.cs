using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PositionBuffer
{
    private static Dictionary<Vector3Int, List<GameObject>> gameObjects;
    public static int size = 300;
    public static int stackSize = 5;

    public static Dictionary<Vector3Int, List<GameObject>> GameObjects { get {
        if(gameObjects == null)Update();
        return gameObjects;
    } set => gameObjects = value; }

    public static void Update(){
        GameObjects = new Dictionary<Vector3Int, List<GameObject>>();
        var levelTransform = GameObject.Find("Levels").transform.GetChild(0);
        foreach (Transform item in levelTransform)
        {
            foreach (var tileItem in item.GetComponentsInChildren<BoxCollider>())
            {
                var tilePos = Utils.Vec3ToInt(tileItem.transform.position);

                if(!GameObjects.ContainsKey(tilePos)){
                    var list = new List<GameObject>();
                    GameObjects.Add(tilePos,list);
                }
                GameObjects[tilePos].Add(item.gameObject);
            }

        }
    }

    internal static List<GameObject> Get(Vector3Int pos)
    {
        if(gameObjects == null){
            Update();
        }
        if(GameObjects.ContainsKey(pos)) return GameObjects[pos];
        return null;
    }


	public static Wall GetWallAtPos(Vector3Int pos) {
		var colliders = Get(pos);
		if(colliders == null)return null;
		for (int i = 0; i < colliders.Count; i++) {
			Wall wall = colliders[i].GetComponentInParent<Wall>();
			if (wall != null) {
				return wall;
			}
		}
		return null;
	}

	public static Wall GetWallAtPos(Vector3 pos) {
		return GetWallAtPos(Utils.Vec3ToInt(pos));
	}

	public static bool WallIsAtPos(Vector3 pos) {
		return WallIsAtPos(Utils.Vec3ToInt(pos));
	}

	public static bool WallIsAtPos(Vector3Int pos) {
		return GetWallAtPos(pos) != null;
	}

	// MOVERS // 
	public static Mover GetMoverAtPos(Vector3 pos) {
		return GetMoverAtPos(Utils.Vec3ToInt(pos));
	}
	public static Mover GetMoverAtPos(Vector3Int pos) {
		var colliders = Get(pos);
		if(colliders == null)return null;
        for (int i = 0; i < colliders.Count; i++) {
			Mover m = colliders[i].GetComponentInParent<Mover>();
			if (m != null) { 
				return m;
			}
        }
		return null;	
	}
}
