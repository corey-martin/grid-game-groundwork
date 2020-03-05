using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using DG.Tweening;
using System.Linq;

[System.Serializable]
public class Tile {
	public Transform transform;
	public Vector3Int pos;
	public Vector3Int rot;
}

public class Mover : MonoBehaviour {

	[HideInInspector] public Vector3 goalPosition;
	public List<Tile> tiles = new List<Tile>();
	[HideInInspector] public bool isFalling = false;
	
	List<Vector3Int> keysToRemove = new List<Vector3Int>();

	public void Start() {
        Init();
	}

    void OnValidate() {
        Init();
    }

    void Init() {
		CreateTiles();
		AddToDict();
    }

	void CreateTiles() {
        tiles.Clear();
		foreach (Transform child in transform) {
			if (child.gameObject.CompareTag("Tile")) {
				Tile tile = new Tile();
				tile.transform = child;
				tile.pos = Vector3Int.RoundToInt(child.position);
				tile.rot = Vector3Int.RoundToInt(child.eulerAngles);
				tiles.Add(tile);
			}
		}
	}

	void AddToDict() {
        foreach (Tile tile in tiles) {
            if (!Game.moveDict.ContainsKey(tile.pos)) {
            	Game.moveDict.Add(tile.pos, this);
            }
        }
	}

	void RemoveFromDict() {
		foreach(var item in Game.moveDict.Where(kvp => kvp.Value == this).ToList()) {
			Game.moveDict.Remove(item.Key);
		}
	}

	public void StoreTileData() {
		RemoveFromDict();
		foreach (Tile tile in tiles) {
			tile.pos = Vector3Int.RoundToInt(tile.transform.position);
			tile.rot = Vector3Int.RoundToInt(tile.transform.eulerAngles);
		}
		AddToDict();
	}

    public void Reset() {
        isFalling = false;
    }

	public bool CanMove(Vector3 dir) {

		foreach (Tile tile in tiles) {
			Vector3Int posToCheck = Vector3Int.RoundToInt(tile.pos + dir);
			if (Utils.WallIsAtPos(posToCheck)) {
				return false;
			}
			Mover m = Utils.GetMoverAtPos(posToCheck);
			if (m != null && m != this) {
				if (m.CanMove(dir)) {
					m.MoveIt(dir);
				} else {
					return false;
				}
			}
		}

		return true;
	}

	public void MoveIt(Vector3 dir) {
		if (!Game.moversToMove.Contains(this)) {
			goalPosition = transform.position + dir;
			Game.moversToMove.Add(this);
		}
	}

    public virtual bool ShouldFall() {   
		if (GroundBelow()) {
			return false;
		}
        return true;
    }

	public virtual void FallStart() {
        if (ShouldFall()) {
            if (!isFalling) {
                isFalling = true;
                Game.Get().movingCount++;
            }
            goalPosition = transform.position + Vector3.forward;
            transform.DOMove(goalPosition, Game.Get().fallTime).OnComplete(FallAgain).SetEase(Ease.Linear);
        } else {
            FallEnd();
        }
	}

	void FallAgain() {
		StartCoroutine(DoFallAgain());
	}

	IEnumerator DoFallAgain() {
		yield return WaitFor.EndOfFrame;
        StoreTileData();
		FallStart();
	}

	public void FallEnd() {
		if (isFalling) {
			isFalling = false;
			Game.Get().movingCount--;
			Game.Get().FallEnd();
		}
	}

    public bool GroundBelow() {
		foreach (Tile tile in tiles) {
			if (GroundBelowTile(tile)) {
				return true;
			}
		}
		return false;
	}

	bool GroundBelowTile(Tile tile) {
		Vector3Int posToCheck = Vector3Int.RoundToInt(tile.pos + Vector3.forward);
		if (Utils.WallIsAtPos(posToCheck)) {
			return true;
		}
		Mover m = Utils.GetMoverAtPos(posToCheck);
		if (m != null && m != this && !m.isFalling) {
			return true;
		}
		return false;
	}

    void OnDrawGizmosSelected() {
        if (!Application.isPlaying) {
            tiles.Clear();
            CreateTiles();
        }
        Gizmos.color = Color.blue;
        foreach (Tile tile in tiles) {    
            Gizmos.DrawWireCube(tile.pos, Vector3.one);
        }
    }
}
