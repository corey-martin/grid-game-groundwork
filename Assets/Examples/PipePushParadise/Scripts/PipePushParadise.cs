using UnityEngine;

public class PipePushParadise : MonoBehaviour
{
    public GameObject winText;
    public static Pipe[] pipes;
    OutletStart[] outletStarts;
    OutletEnd[] outletEnds;

    void Awake() {
        pipes = FindObjectsOfType<Pipe>();
        outletStarts = FindObjectsOfType<OutletStart>();
        outletEnds = FindObjectsOfType<OutletEnd>();
    }

    void Start() {
        CheckWinCondition();
    }

    void OnEnable() {
        Game.onMoveComplete += CheckWinCondition;
        Game.onUndo += CheckWinCondition;
        Game.onReset += CheckWinCondition;
    }

    void OnDisable() {
        Game.onMoveComplete -= CheckWinCondition;
        Game.onUndo -= CheckWinCondition;
        Game.onReset -= CheckWinCondition;
    }

    void CheckWinCondition() {
        foreach (Pipe pipe in pipes) {
            pipe.StopFlow();
        }
        foreach (OutletStart outlet in outletStarts) {
            outlet.StartFlow();
        }
        foreach (Pipe pipe in pipes) {
            if (!pipe.isFlowing) {
                pipe.DisableWater();
            }
        }
        winText.SetActive(won);
    }

    bool won {
        get {
            foreach(OutletEnd outlet in outletEnds) {
                if (!outlet.isFlowing) {
                    return false;
                }
            }
            return true;
        }
    }
}
