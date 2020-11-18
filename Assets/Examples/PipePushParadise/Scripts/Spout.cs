using UnityEngine;

public class Spout : MonoBehaviour
{
    public Pipe pipe;
    public Transform target;
    public Transform origin;
    public GameObject water;
    public bool debug = false;
    bool isFlowing;

    public Vector3Int direction {
        get {
            return Utils.Vec3ToInt(target.position - origin.position);
        }
    }

    public void StartFlow() {
        if (isFlowing) return;
        isFlowing = true;
        water.SetActive(true);
        pipe.StartFlow(this);
        Spout[] spouts = PipeUtils.GetSpoutsAtPos(target.position);
        foreach (Spout s in spouts) {
            if (s != null && s.direction == -direction) {
                s.StartFlow();
            }
        }
    }

    public void StopFlow() {
        isFlowing = false;
    }
}
