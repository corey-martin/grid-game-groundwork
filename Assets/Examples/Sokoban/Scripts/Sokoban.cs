using UnityEngine;

public class Sokoban : MonoBehaviour
{
    public GameObject winText;

    void Start() {
        Game.isPolyban = false;
        winText.SetActive(false);
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
        winText.SetActive(won);
    }

    bool won {
        get {
            foreach (Mover m in Game.movers) {
                if (m.isPlayer) continue;
                bool onTarget = false;
                foreach (Tile t in m.tiles) {
                    Collider[] colliders = Utils.GetCollidersAt(t.pos);
                    foreach (Collider col in colliders) {
                        if (col.transform.CompareTag("Target")) {
                            onTarget = true;
                        }
                    }
                }
                if (!onTarget) {
                    return false;
                }
            }
            return true;
        }
    }
}
