using System.Collections.Generic;
using UnityEngine;

public class PipeUtils
{
    static List<Spout> spouts = new List<Spout>();

    public static Spout[] GetSpoutsAtPos(Vector3 pos) {
        spouts.Clear();
		foreach (Pipe pipe in PipePushParadise.pipes) {
            foreach (Spout spout in pipe.spouts) {
                if (Utils.VectorRoughly(pos, spout.transform.position)) {
                    spouts.Add(spout);
                }
            }
        }	
        return spouts.ToArray();
	}
}
