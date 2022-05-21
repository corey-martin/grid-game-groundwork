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
