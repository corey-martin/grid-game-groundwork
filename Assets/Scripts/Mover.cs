using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using DG.Tweening;

public class Mover : MonoBehaviour {

	[HideInInspector] public Vector3 goalPosition;
	public List<Tile> tiles = new List<Tile>();
	[HideInInspector] public bool isFalling = false;
	public bool isPlayer { get { return CompareTag("Player"); }}

	void Start() {
		CreateTiles();
	}

	void CreateTiles() {
        tiles.Clear();
		foreach (Transform child in transform) {
			if (child.gameObject.CompareTag("Tile")) {
				Tile tile = new Tile();
				tile.t = child;
				tiles.Add(tile);
			}
		}
	}

    public void Reset() {
        isFalling = false;
    }

	public virtual bool CanMove(Vector3 dir) {

		foreach (Tile tile in tiles) {
			Vector3Int posToCheck = Vector3Int.RoundToInt(tile.pos + dir);
			if (PositionBuffer.WallIsAtPos(posToCheck)) {
				return false;
			}
			Mover m = PositionBuffer.GetMoverAtPos(posToCheck);
			if (m != null && m != this) {
				if (!isPlayer && !Game.isPolyban) {
					return false;
				}
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
			if (Utils.Roughly(tile.pos.z, 0)) {
				return true;
			}
			if (GroundBelowTile(tile)) {
				return true;
			}
		}
		return false;
	}

	bool GroundBelowTile(Tile tile) {
		Vector3Int posToCheck = Vector3Int.RoundToInt(tile.pos + Vector3.forward);
		if (PositionBuffer.WallIsAtPos(posToCheck)) {
			return true;
		}
		Mover m = PositionBuffer.GetMoverAtPos(posToCheck);
		if (m != null && m != this && !m.isFalling) {
			return true;
		}
		return false;
	}

    void OnDrawGizmosSelected() {
        if (!Application.isPlaying) {
            CreateTiles();
        }
        Gizmos.color = Color.blue;
        foreach (Tile tile in tiles) {    
            Gizmos.DrawWireCube(tile.pos, Vector3.one);
        }
    }
}
