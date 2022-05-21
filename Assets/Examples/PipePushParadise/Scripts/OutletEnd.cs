using UnityEngine;

public class OutletEnd : Pipe
{
	public override bool CanMoveToward(Vector3Int dir) {
        return false;
    }
}
