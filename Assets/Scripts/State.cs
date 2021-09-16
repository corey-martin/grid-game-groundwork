using System.Collections.Generic;
using UnityEngine;

public class State
{
    public struct MoverToTrack {
        public Mover mover;
	    public Vector3 initialPos;
        public Vector3 initialRot;
        public List<Vector3Int> positions;
        public List<Vector3Int> rotations;
    }

    public static List<MoverToTrack> moversToTrack = new List<MoverToTrack>();
	public static int undoIndex;

    public static void AddMover(Mover mover) {
        MoverToTrack newMover = new MoverToTrack();
        newMover.mover = mover;
        newMover.initialPos = mover.transform.position;
        newMover.initialRot = mover.transform.eulerAngles;
        newMover.positions = new List<Vector3Int>();
        newMover.rotations = new List<Vector3Int>();
        moversToTrack.Add(newMover);
    }

    public static void Init() {
        undoIndex = 0;
        moversToTrack.Clear();
    }

	public static void AddToUndoStack() {
        foreach (MoverToTrack m in moversToTrack) {
		    m.positions.Add(Vector3Int.RoundToInt(m.mover.transform.position));
		    m.rotations.Add(Vector3Int.RoundToInt(m.mover.transform.eulerAngles));
        }
    }

	public static void RemoveFromUndoStack() {
        foreach (MoverToTrack m in moversToTrack) {
            m.positions.RemoveAt(m.positions.Count - 1);
            m.rotations.RemoveAt(m.rotations.Count - 1);
            m.mover.transform.position = m.positions[m.positions.Count - 1];
            m.mover.transform.eulerAngles = m.rotations[m.rotations.Count - 1];
        }
	}

    public static void OnMoveComplete() {
        undoIndex++;
        AddToUndoStack();
    }

	public static void DoUndo() {
		if (undoIndex > 0) {
            undoIndex--;
            RemoveFromUndoStack();
		}
	}

	public static void DoReset() {
        foreach (MoverToTrack m in moversToTrack) {
		    m.mover.transform.position = m.initialPos;
		    m.mover.transform.eulerAngles = m.initialRot;
        }
		OnMoveComplete();
	}
}
