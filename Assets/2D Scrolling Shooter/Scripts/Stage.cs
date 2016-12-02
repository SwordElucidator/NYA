using UnityEngine;
using System.Collections;
using Dreamteck.Splines;

public class Stage : MonoBehaviour {

    // Movement
    public SplineComputer[] computers;
    public float[] speed;
    public float[] intervals; // intervals[0] is before the computers[0]
    public bool repeat = true;

    // Curtain
    public Curtain[] curtains;
    public float[] curtain_intervals; // curtain_intervals[0] is before the curtains[0]
}
