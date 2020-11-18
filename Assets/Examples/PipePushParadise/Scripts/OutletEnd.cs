using UnityEngine;

public class OutletEnd : Pipe
{
	public override bool CanMove(Vector3 dir) {
        return false;
    }
}
