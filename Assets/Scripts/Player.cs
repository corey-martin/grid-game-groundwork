using UnityEngine;

public class Player : Mover {

	public static Player instance;
	public static Player Get() { return instance; }
	Vector3 direction = Vector3.zero;

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
			if (direction == Vector3.right || direction == Vector3.left) {
				hor = 0;
			} else {
				ver = 0;
			}
		}

		if (hor == 1) {
			direction = Vector3.right;
		} else if (hor == -1) { 
			direction = Vector3.left;
		} else if (ver == -1) {
			direction = Vector3.down;
		} else if (ver == 1) {
			direction = Vector3.up;
		}

		if (CanMove(direction)) {
			MoveIt(direction);
			Game.Get().MoveStart(direction);
		} else {
			Game.moversToMove.Clear();
		}
	}
}
