using System.Linq;
using UnityEngine;

public class Sokoban : MonoBehaviour
{
    public GameObject winText;

    void Start() {
        Game.isPolyban = false;
        winText.SetActive(false);
    }

    void OnEnable() {
        EventManager.onMoveComplete += CheckWinCondition;
        EventManager.onUndo += CheckWinCondition;
        EventManager.onReset += CheckWinCondition;
    }

    void OnDisable() {
        EventManager.onMoveComplete -= CheckWinCondition;
        EventManager.onUndo -= CheckWinCondition;
        EventManager.onReset -= CheckWinCondition;
    }

    void CheckWinCondition() {
        winText.SetActive(won);
    }

    private static bool won {
        get
        {
            return Game.movers.All(m => m.isPlayer || MoverOnTarget(m));
        }
    }

    private static bool MoverOnTarget(Mover m)
    {
        return m.tiles.Any(t => Utils.TaggedObjIsAtPos(t.pos, "Target"));
    }
}
