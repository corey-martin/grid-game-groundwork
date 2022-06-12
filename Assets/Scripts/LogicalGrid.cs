using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LogicalGrid
{
    private Dictionary<Vector3Int, HashSet<GameObject>> Contents;

    public LogicalGrid()
    {
        Contents = new Dictionary<Vector3Int, HashSet<GameObject>>();
    }

    public void SyncContents(GameObject[] tiles)
    {
        foreach (var kv in Contents)
            kv.Value.Clear();
        foreach (var tile in tiles)
        {
            var pos = Vector3Int.RoundToInt(tile.transform.position);
            if (!Contents.ContainsKey(pos))
                Contents[pos] = new HashSet<GameObject>();
            Contents[pos].Add(tile);
        }
    }

    public HashSet<GameObject> GetContentsAt(Vector3Int pos)
    {
        if (!Contents.ContainsKey(pos))
            Contents[pos] = new HashSet<GameObject>(); // weird side effect...
        return Contents[pos];
    }
}
