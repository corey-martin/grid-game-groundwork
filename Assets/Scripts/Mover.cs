using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using DG.Tweening;

public class Mover : MonoBehaviour {

	public List<Tile> tiles = new List<Tile>();
	[HideInInspector] public bool isFalling = false;
	public bool isPlayer { get { return CompareTag("Player"); }}

	// During a movement cycle, what's the next move (as a difference
	// from its current position) that this Mover will try to make?
	private Vector3Int PlannedMove;

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

	public void Stop()
	{
		PlannedMove = Vector3Int.zero;
	}

	public Vector3Int Pos()
	{
		return Vector3Int.RoundToInt(transform.position);
	}

	// Try to plan a move in the indicated direction, if that move
	// is valid.
	public bool TryPlanMove(Vector3Int dir)
	{
		if (!CanMoveToward(dir))
			return false;
		PlanMove(dir);
		return true;
	}

	private void PlanMove(Vector3Int dir)
	{
		// Optional optimization - avoid redundant pushes
		// with many multi-tile movers. Slightly fragile.
		if (PlannedMove == dir) return;
		
		PlannedMove = dir;
		PlanPushes(dir);
	}

	public bool HasPlannedMove()
	{
		return PlannedMove != Vector3Int.zero;
	}

	// If there are other movers in the given direction,
	// push them in the same direction. Does not check whether
	// the move is possible - assumes that CanMoveToward()
	// already checked.
	private void PlanPushes(Vector3Int dir)
	{
		foreach (Tile tile in tiles) {
			Vector3Int posToCheck = tile.pos + dir;
			Mover m = Utils.GetMoverAtPos(posToCheck);
			if (m == null || m == this) continue;
			m.PlanMove(dir);
		}
	}

	// Perform the currently planned move (if any).
	public bool ExecuteLogicalMove()
	{
		if (PlannedMove == Vector3Int.zero)
			return false;
		
		transform.position = Pos() + PlannedMove;
		PlannedMove = Vector3Int.zero;
		return true;
	}

	// Handle effects that happen after moving, such as
	// planning to fall.
	public void FinalizeLogicalMove()
	{
		if (ShouldFall())
			PlanMove(Utils.forward);
	}

	public virtual bool CanMoveToward(Vector3Int dir) {
		foreach (Tile tile in tiles) {
			Vector3Int posToCheck = tile.pos + dir;
			if (Utils.WallIsAtPos(posToCheck)) {
				return false;
			}
			Mover m = Utils.GetMoverAtPos(posToCheck);
			// Movers don't block themselves.
			if (m == null || m == this)
				continue;
			// Only the player can push other movers,
			// unless we're in polyban mode.
			if (!isPlayer && !Game.isPolyban)
				return false;
			// XXX: could this cause an infinite loop with, say,
			// a U-shaped block and a single block inside, or two
			// interlocking U-blocks? We can fix this by passing
			// in (& ignoring) the set of already checked movers.
			if (!m.CanMoveToward(dir))
				return false;
		}

		return true;
	}

    public virtual bool ShouldFall() {   
		if (GroundBelow()) {
			return false;
		}
        return true;
    }

    public bool GroundBelow() {
		foreach (Tile tile in tiles)
		{
			if (tile.pos.z == 0)
				return true;
			if (GroundBelowTile(tile)) {
				return true;
			}
		}
		return false;
	}

	bool GroundBelowTile(Tile tile) {
		Vector3Int posToCheck = tile.pos + Utils.forward;
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
            CreateTiles();
        }
        Gizmos.color = Color.blue;
        foreach (Tile tile in tiles) {    
            Gizmos.DrawWireCube(tile.pos, Vector3.one);
        }
    }
}
