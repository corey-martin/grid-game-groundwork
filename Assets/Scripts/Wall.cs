using System.Collections.Generic;
using UnityEngine;

public class Wall : MonoBehaviour
{
    public bool isStatic = true;
    public List<Transform> tiles = new List<Transform>();

    void OnValidate() {
        CreateTiles();
    }

    void CreateTiles() {
        tiles.Clear();
		foreach (Transform child in transform) {
			if (child.gameObject.CompareTag("Tile")) {
                tiles.Add(child);
			}
		}
    }

    void OnDrawGizmosSelected() {
        if (!Application.isPlaying) {
            CreateTiles();
        }
        Gizmos.color = Color.yellow;
        foreach (Transform tile in tiles) {    
            Gizmos.DrawWireCube(tile.position, tile.localScale);
        }
    }
}
