using UnityEngine;

public class Player : Mover {

	public static Player instance;
	public static Player Get() { return instance; }
	Vector3Int direction = Vector3Int.zero;

	void Awake() {
		instance = this;
	}
	
	void Update () {
		if (CanInput()) {
			CheckInput();
		}
	}

	public bool CanInput() {
		return !Game.Get().isMoving && !Game.Get().holdingUndo;
	}

	public void CheckInput() {

		float hor = Input.GetAxisRaw("Horizontal");
		float ver = Input.GetAxisRaw("Vertical");

		if (hor == 0 && ver == 0) {
			return;
		}

		if (hor != 0 && ver != 0) {
			if (direction == Vector3Int.right || direction == Vector3Int.left) {
				hor = 0;
			} else {
				ver = 0;
			}
		}

		if (hor == 1) {
			direction = Vector3Int.right;
		} else if (hor == -1) { 
			direction = Vector3Int.left;
		} else if (ver == -1) {
			direction = Vector3Int.down;
		} else if (ver == 1) {
			direction = Vector3Int.up;
		}

		if (TryPlanMove(direction))
			Game.Get().MoveStart();
	}
}
