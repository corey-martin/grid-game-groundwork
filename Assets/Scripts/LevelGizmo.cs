using UnityEngine;

public class LevelGizmo : MonoBehaviour
{
    public static Vector3 pos { get; private set; }
    static Color color = new Color(2f, 2f, 2f);
    public static bool drawEnabled { get; private set; }

    public static void UpdateGizmo(Vector3 v, Color c) {
        pos = v;
        color = c;
    }

    public static void Enable(bool b) {
        drawEnabled = b;
    }

    void OnDrawGizmos() {
        if (drawEnabled) {
            Gizmos.color = color;
            Gizmos.DrawWireCube(pos, Vector3.one);
            Gizmos.DrawWireCube(pos + (Vector3.one * 0.01f), Vector3.one);
            Gizmos.DrawWireCube(pos - (Vector3.one * 0.01f), Vector3.one);
        }
    }
}
