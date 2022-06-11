using UnityEngine;

public class Tile {
	public Transform t;
	public Vector3Int pos { get { return Vector3Int.RoundToInt(t.position); } }
	public Vector3 rot { get { return t.eulerAngles; } }
}
