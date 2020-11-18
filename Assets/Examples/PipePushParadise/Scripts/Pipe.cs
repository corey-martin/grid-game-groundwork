
public class Pipe : Mover
{
    public Spout[] spouts;
    public bool isFlowing;

    public void StartFlow(Spout originSpout = null) {
        if (isFlowing) return;
        isFlowing = true;
        foreach (Spout spout in spouts) {
            if (spout == originSpout) continue;
            spout.StartFlow();
        }
    }

    public void StopFlow() {
        isFlowing = false;
        foreach (Spout spout in spouts) {
            spout.StopFlow();
        }
    }

    public void DisableWater() {
        foreach (Spout spout in spouts) {
            spout.water.SetActive(false);
        }
    }
}
