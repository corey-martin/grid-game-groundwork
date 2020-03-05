using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using DG.Tweening;

public class Game : MonoBehaviour {

	public static Game instance;
	public static Game Get() { return instance; }

	public static Mover[] movers;
	public static List<Mover> moversToMove = new List<Mover>();

	public float moveTime = 0.18f; // time it takes to move 1 unit
	public float fallTime = 0.1f; // time it takes to fall 1 unit

	public static Dictionary<Vector3Int, Mover> moveDict = new Dictionary<Vector3Int, Mover>();
	public static Dictionary<Vector3Int, Wall> wallDict = new Dictionary<Vector3Int, Wall>();

	public static bool isMoving = false;
	public int movingCount = 0;
	public bool holdingUndo = false;

	void Awake() {
		instance = this;
		moveDict.Clear();
		wallDict.Clear();

		if (Application.isEditor && !SaveData.initialized) {
			SaveData.LoadGame(1);
		}
	}

	void Start() {
		GetRefs();
		State.AddToUndoStack();
		isMoving = false;
	}

	public static void GetRefs() {
		movers = FindObjectsOfType<Mover>();
		State.Init();
		foreach (Mover mover in movers) {
			State.AddMover(mover);
		}
	}

	public void EditorRefresh() {
		movers = FindObjectsOfType<Mover>();
		wallDict.Clear();
		Wall[] walls = FindObjectsOfType<Wall>();
		foreach (Wall wall in walls) {
			wall.Init();
		}
		UpdateMovers();
	}

	void Update() {
		if (Input.GetKeyDown(KeyCode.Z)) {
			holdingUndo = true;
			DoUndo();
			DOVirtual.DelayedCall(0.75f, UndoRepeat);
			
		} else if (Input.GetKeyDown(KeyCode.R)) {
			Reset();
		}
		if (Input.GetKeyUp(KeyCode.Z)) {
			StartCoroutine(StopUndoing());
		}
	}

	IEnumerator StopUndoing() {
		yield return WaitFor.EndOfFrame;
		holdingUndo = false;
	}

	public void Refresh() {
		isMoving = false;
		UpdateMovers();
		moversToMove.Clear();
		movingCount = 0;
	}

    void Reset() {
		DOTween.KillAll();
		isMoving = false;
		State.DoReset();
		Refresh();
    }

	public void CompleteMove() {
		State.OnMoveComplete();
		moversToMove.Clear();
	}

	void DoUndo() {
		if (State.undoIndex > 0) {
			DOTween.KillAll();
			if (isMoving) {
				CompleteMove();
			}
			isMoving = false;
			State.DoUndo();
			Refresh();
		}
	}

	void UndoRepeat() {
		if (Input.GetKey(KeyCode.Z) && holdingUndo) {
			DoUndo();
			DOVirtual.DelayedCall(0.075f, UndoRepeat);
		}
	}

	public void MoveStart(Vector3 dir) {
		isMoving = true;
		foreach (Mover m in moversToMove) {
			movingCount++;
			m.transform.DOMove(m.goalPosition, moveTime).OnComplete(MoveEnd).SetEase(Ease.Linear);
		}
	}

	public void MoveEnd() {
		movingCount--;
		if (movingCount == 0) {
			FallStart();
		}
	}
	
	public void FallStart() {
		isMoving = true;
		UpdateMovers();
		movers = movers.OrderBy((c) => -c.transform.position.z).ToArray();

		foreach (Mover m in movers) {
			m.FallStart();
		}
		if (movingCount == 0) {
			FallEnd();
		}
	}

	public void FallEnd() {
		if (movingCount == 0) {
			Refresh();
			CompleteMove();
		}
	}

	void UpdateMovers() {
		moveDict.Clear();
		foreach (Mover m in movers) {
			m.Reset();
			m.StoreTileData();
		}
	}
}
