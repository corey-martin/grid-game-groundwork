using System;
using UnityEngine;

public class EventManager
{
    public static Action<string> onLevelStarted;
    public static Action<string> onLevelQuit;
    public static Action<string> onLevelComplete;
    public static Action<Vector3> onMoveStart;
    public static Action onMoveComplete;
    public static Action onPush;
    public static Action onUndo;
    public static Action onReset;
    public static Action onUISelect;
    public static Action onUISubmit;
    
}
