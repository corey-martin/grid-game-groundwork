using UnityEngine;

public class Tile {
	public Transform t;
	public Vector3 pos { get { return t.position; } }
	public Vector3 rot { get { return t.eulerAngles; } }
}
