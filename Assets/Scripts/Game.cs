using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using DG.Tweening;

public class Game : MonoBehaviour {

	private static Game instanceRef;
	public static Game instance {
		get {
			if (instanceRef == null) {
				instanceRef = FindObjectOfType<Game>();
			}
			return instanceRef;
		}
	}

	public LogicalGrid Grid = new LogicalGrid();

	public static List<Mover> movers = new List<Mover>();
	public static List<Wall> walls = new List<Wall>();

	public float moveTime = 0.18f; // time it takes to move 1 unit
	public float fallTime = 0.1f; // time it takes to fall 1 unit
	public float moveBufferSpeedupFactor = 0.5f; //degree of speedup due to buffered inputs
	
	public Ease moveEase = Ease.Linear;

	private int movingCount = 0;
	private List<List<MoverPos>> PlannedMoves = new List<List<MoverPos>>();

	public bool holdingUndo { get; private set; } = false;
	public static bool isPolyban = true;
	public bool blockInput = false;

	void Awake() {
		if (instanceRef == null || instanceRef == this) {
			instanceRef = this;
			Application.targetFrameRate = 60;

			if (Application.isEditor && !SaveData.initialized) {
				SaveData.LoadGame(1);
				SyncGrid();
			}
		} else {
			Debug.LogError("More than 1 Game class in scene");
		}
	}

	void Start() {
		StartCoroutine(InitAfterFrame());
	}

	IEnumerator InitAfterFrame() {
		blockInput = true;
		yield return WaitFor.EndOfFrame;
		SetReferences();
		State.Init();
		foreach (Mover mover in movers) {
			State.AddMover(mover);
		}
		State.AddToUndoStack();
		blockInput = false;
	}

	public void EditorRefresh() {
		SetReferences();
	}

	void SetReferences() {
		movers.Clear();
		walls.Clear();
		movers = FindObjectsOfType<Mover>().ToList();
		walls = FindObjectsOfType<Wall>().ToList();
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
		movingCount = 0;
		Player.instance.ClearInputBuffer();
		PlannedMoves.Clear();
		foreach (var mover in movers)
			mover.Stop();
		SyncGrid();
	}

	public void SyncGrid()
	{
		var tiles = GameObject.FindGameObjectsWithTag("Tile");
		Grid.SyncContents(tiles);
	}
	
	public bool isMoving { get { return movingCount > 0; } }

	/////////////////////////////////////////////////////////////////// UNDO / RESET

    void DoReset() {
		DOTween.KillAll();
		State.DoReset();
		Refresh();
		EventManager.onReset?.Invoke();
    }

	void DoUndo()
	{
		if (State.undoIndex <= 0)
			return;
		
		DOTween.KillAll();
		if (isMoving) {
			CompleteMove();
		}
		State.DoUndo();
		Refresh();
		EventManager.onUndo?.Invoke();
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

	private struct MoverPos
	{
		public Mover m;
		public Vector3Int Pos;

		public MoverPos(Mover mov)
		{
			m = mov;
			Pos = mov.Pos();
		}
	};

	// Build a list of positions for each mover.
	private List<MoverPos> GetMoverPositions()
	{
		var lerps = new List<MoverPos>();
		foreach (var mover in movers) {
			if (mover != null) {
				lerps.Add(new MoverPos(mover));
			}
		}
		return lerps;
	}

	// MoveStart calculates the 'logical' effects of a player action,
	// building up a list of movements. Those are executed visually afterward.
	public void MoveStart(bool doPostMoveEffects = true)
	{
		// For each movement 'cycle', we store the positions of all movers.
		// If we wanted to, we could compress this down to only those movers which have
		// changed positions, though it'd be some extra work to tell when things should
		// start moving. (This might be important for large overworlds.)
		PlannedMoves.Clear();
		
		for (int i = 0; i < 999 /*safety*/ && movers.Any(m => m.HasPlannedMove()); ++i)
		{
			PlannedMoves.Add(GetMoverPositions());
			bool isPushing = false;
			
			// Execute planned moves.
			var moved = false;
			foreach (var mover in movers) {
				if (mover.ExecuteLogicalMove()) {
					if (!mover.isPlayer) isPushing = true;
					moved = true;
				}
			}
			
			// Update the contents of the grid.
			if (moved)
			{
				if (isPushing) EventManager.onPush?.Invoke();

				// NOTE: if we want improved performance, we can track
				// which movers have actually moved and pass them in to
				// SyncGrid, only changing their previous & new destinations
				// rather than regenerating the whole grid.
				SyncGrid();
			}
			
			// Check for follow-ups, like things starting or continuing to fall.
			if (doPostMoveEffects) {
				foreach (var mover in movers)
					mover.DoPostMoveEffects();
			}
		}
		
		PlannedMoves.Add(GetMoverPositions());

		// Finally, start animating the moves we just calculated.
		StartMoveCycle(false);
		// After they're all done or cancelled, we'll run CompleteMove().
	}

	public Vector3 GetPlayerPosition() {
		if (PlannedMoves.Count > 0) {
			foreach (var move in PlannedMoves[0]) {
				return move.Pos;
			}
		}
		return Player.instance.transform.position;
	}

	private void StartMoveCycle(bool falling)
	{
		foreach (var move in PlannedMoves[0])
			move.m.transform.position = move.Pos;
		PlannedMoves.RemoveAt(0);
		if (PlannedMoves.Count == 0)
		{
			CompleteMove();
			return;
		}

		if (falling) { Player.instance.ClearInputBuffer(); }

		float dur;
		if (falling)
		{
			dur = fallTime;
		}
		else
		{
			dur = moveTime / (Player.instance.InputBuffer.Count()*moveBufferSpeedupFactor + 1); // increase the animation speed when moves are buffered
		}
		foreach (var move in PlannedMoves[0])
		{
			if (move.Pos == move.m.Pos()) continue;
			++movingCount;
			move.m.transform.DOMove(move.Pos, dur).OnComplete(MoveEnd).SetEase(moveEase);
		}
	}

	public void MoveEnd() {
		movingCount--;
		if (movingCount == 0)
		{
			// We assume that all move cycles after the first are falls.
			// This won't be true for all games (eg those with conveyors,
			// slippery ice, etc), so you'll need to adjust this.
			StartMoveCycle(true);
		}
	}

	public void CompleteMove() {
		State.OnMoveComplete();
		EventManager.onMoveComplete?.Invoke();
	}
}
