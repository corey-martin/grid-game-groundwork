using System.Collections.Generic;
using UnityEngine;

public class Wall : MonoBehaviour
{
    public bool isStatic = true;
    public List<Vector3Int> tiles = new List<Vector3Int>();

    public List<Vector3Int> GetTiles() {
        if (!isStatic) {
            tiles.Clear();
            GenerateTiles();
        }
        return tiles;
    }

    void Start() {
        Init();
    }

    void OnValidate() {
        Init();
    }

    public void Init() {
        GenerateTiles();
        foreach (Vector3Int tile in tiles) {
            if (!Game.wallDict.ContainsKey(tile)) {
                Game.wallDict.Add(tile, this);
            }
        }
    }

    void GenerateTiles() {
        tiles.Clear();
		foreach (Transform child in transform) {
			if (child.gameObject.CompareTag("Tile")) {
                tiles.Add(Vector3Int.RoundToInt(child.position));
			}
		}
    }

    void OnDrawGizmosSelected() {
        if (!Application.isPlaying) {
            GenerateTiles();
        }
        Gizmos.color = Color.yellow;
        foreach (Vector3Int tile in tiles) {    
            Gizmos.DrawWireCube(tile, Vector3.one);
        }
    }
}
