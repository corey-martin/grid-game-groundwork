using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using DG.Tweening;

public class Game : MonoBehaviour {

	
	public delegate void GameEvent();
	public static GameEvent onUndo;
	public static GameEvent onReset;
	public static GameEvent onMoveComplete;

	public static Game instance;
	public static Game Get() { return instance; }

	public LogicalGrid Grid = new LogicalGrid();

	public static Mover[] movers;
	public static Wall[] walls;
	public static List<Mover> moversToMove = new List<Mover>();

	public float moveTime = 0.18f; // time it takes to move 1 unit
	public float fallTime = 0.1f; // time it takes to fall 1 unit

	public static bool isMoving = false;
	public int movingCount = 0;
	public bool holdingUndo = false;
	public static bool isPolyban = true;

	void Awake() {
		instance = this;
		Application.targetFrameRate = 60;

		if (Application.isEditor && !SaveData.initialized) {
			SaveData.LoadGame(1);
			SyncGrid();
		}
	}

	void Start() {
		movers = FindObjectsOfType<Mover>();
		State.Init();
		foreach (Mover mover in movers) {
			State.AddMover(mover);
		}
		State.AddToUndoStack();
		isMoving = false;
	}

	public void EditorRefresh() {
		movers = FindObjectsOfType<Mover>();
		walls = FindObjectsOfType<Wall>();
		SyncGrid();
	}

	void Update() {
		if (Input.GetKeyDown(KeyCode.Z)) {
			holdingUndo = true;
			DoUndo();
			DOVirtual.DelayedCall(0.75f, UndoRepeat);
			
		} else if (Input.GetKeyDown(KeyCode.R)) {
			DoReset();
		}
		if (Input.GetKeyUp(KeyCode.Z)) {
			StartCoroutine(StopUndoing());
		}
	}

	public void Refresh() {
		isMoving = false;
		moversToMove.Clear();
		movingCount = 0;
		SyncGrid();
	}

	public void SyncGrid()
	{
		var tiles = GameObject.FindGameObjectsWithTag("Tile");
		Grid.SyncContents(tiles);
	}

	/////////////////////////////////////////////////////////////////// UNDO / RESET

    void DoReset() {
		DOTween.KillAll();
		isMoving = false;
		State.DoReset();
		Refresh();
		if (onReset != null) {
			onReset();
		}
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
			if (onUndo != null) {
				onUndo();
			}
		}
	}

	void UndoRepeat() {
		if (Input.GetKey(KeyCode.Z) && holdingUndo) {
			DoUndo();
			DOVirtual.DelayedCall(0.075f, UndoRepeat);
		}
	}

	IEnumerator StopUndoing() {
		yield return WaitFor.EndOfFrame;
		holdingUndo = false;
	}

	/////////////////////////////////////////////////////////////////// MOVE

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
			SyncGrid();
			FallStart();
		}
	}
	
	public void FallStart() {
		isMoving = true;
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

	public void CompleteMove() {
		State.OnMoveComplete();
		if (onMoveComplete != null) {
			onMoveComplete();
		}
	}
}
