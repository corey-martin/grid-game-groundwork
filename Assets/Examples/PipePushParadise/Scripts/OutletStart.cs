using UnityEngine;

public class OutletStart : Pipe {

	public override bool CanMoveToward(Vector3Int dir) {
        return false;
    }
}
