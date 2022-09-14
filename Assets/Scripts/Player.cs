using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class Player : Mover {

	public static Player instance { get; private set; }
	Vector3Int direction = Vector3Int.zero;

	float prevHorInput = 0;
	float prevVerInput = 0;
	
	public List<Vector3Int> InputBuffer = new List<Vector3Int>();

	void Awake() {
		instance = this;
	}

	void Update() {
		if (!Game.instance.holdingUndo)	{
			BufferInput();
		}

		if (CanInput())	{
			CheckBufferedInput();
		}
	}

	public bool CanInput() {
		return !Game.instance.isMoving && !Game.instance.holdingUndo;
	}

	public void ClearInputBuffer() {
		InputBuffer.Clear();
		prevHorInput = 0;
		prevVerInput = 0;
		direction = Vector3Int.zero;
	}

	public void BufferInput() {

		float newHor = Input.GetAxisRaw("Horizontal");
		float newVer = Input.GetAxisRaw("Vertical");

		bool shouldBufferInput =
			(newHor != prevHorInput || newVer != prevVerInput) && //input is different from last time it was checked
			!((newHor == 0 && newVer == prevVerInput) || (newVer == 0 && newHor == prevHorInput)); //the change isn't just due to releasing a key

		Vector3Int dir = Vector3Int.zero;

		if (InputBuffer.Count == 0)
		{
			if (shouldBufferInput || CanInput() ) {
				dir = CalculateNewDirFromInput(direction);
			}
		}
		else
		{
			if (shouldBufferInput) {
				dir = CalculateNewDirFromInput(InputBuffer.Last());
			}
		}

		if (dir != Vector3Int.zero)	{
			InputBuffer.Add(dir);
		}

		prevHorInput = newHor;
		prevVerInput = newVer;
	}

	public void CheckBufferedInput() {

		if (InputBuffer.Count == 0) {
			return;
		}

		direction = InputBuffer.First();
		InputBuffer.RemoveAt(0);

		if (TryPlanMove(direction))	{
			Game.instance.MoveStart();
		}

	}

	public Vector3Int CalculateNewDirFromInput(Vector3Int currentDir) {

		float hor = Input.GetAxisRaw("Horizontal");
		float ver = Input.GetAxisRaw("Vertical");

		if (hor == 0 && ver == 0) {
			return Vector3Int.zero;
		}

		if (hor != 0 && ver != 0) {
			if (currentDir == Vector3Int.right || currentDir == Vector3Int.left) {
				hor = 0;
			}
			else {
				ver = 0;
			}
		}

		if (hor == 1) {
			return Vector3Int.right;
		}
		else if (hor == -1) {
			return Vector3Int.left;
		}
		else if (ver == -1) {
			return Vector3Int.down;
		}
		else if (ver == 1) {
			return Vector3Int.up;
		}
		
		return Vector3Int.zero;
	}

}
