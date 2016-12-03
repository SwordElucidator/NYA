using UnityEngine;
using System.Collections;
using Dreamteck.Splines;

public class Stage : MonoBehaviour {

    // Movement
    public SplineComputer[] computers;
    public float[] speeds;
    public float[] intervals; // intervals[0] is before the computers[0]
    public float stageInterval = 2F;
    public bool repeat = true;

    // Curtain
    protected Curtain[] curtains;
    public float[] curtain_intervals; // curtain_intervals[0] is before the curtains[0]

    void OnEnable()
    {

        if (transform.FindChild("Curtains") != null)
        {
            curtains = new Curtain[transform.FindChild("Curtains").childCount];
            for (int i = 0; i < transform.FindChild("Curtains").childCount; i++)
            {
                curtains[i] = transform.FindChild("Curtains").GetChild(i).GetComponent<Curtain>();
            }
        }

        StartCoroutine("doStageCurtain");
        StartCoroutine("doStageMovement");

    }

    void OnDisable()
    {
        StopCoroutine("doStageCurtain");
        StopCoroutine("doStageMovement");
    }

    IEnumerator doStageMovement()
    {
        int len = intervals.Length;
        if (len == 0)
        {
            yield break;
        }

        do
        {
            int current = 0;
            while (current < len)
            {
                yield return new WaitForSeconds(intervals[current]);
                // ALERT, we should know that transform's parent is Stages and it's parent is Enemy or Boss
                if (transform.parent.parent.GetComponent<SplineFollower>() == null)
                {
                    transform.parent.parent.gameObject.AddComponent<SplineFollower>();
                }
                SplineFollower follower = transform.parent.parent.GetComponent<SplineFollower>();
                follower.computer = computers[current];
                follower.followSpeed = 10;
                follower.autoFollow = true;
                follower.applyRotation = false;
                follower.applyScale = false;
                follower.applyPosition = true;
                follower.findStartPoint = false;
                follower.startPercent = 0;
                follower.averageResultVectors = false;
                follower.enabled = true;
                //transform.parent.parent.position = follower.EvaluatePosition(0);

                follower.Rebuild(true);
                follower.Restart();
                follower.SetPercent(0);
                follower.Move(1);
                current++;
            }
            yield return new WaitForSeconds(stageInterval);
        } while (repeat);

    }

    IEnumerator doStageCurtain()
    {
        int len = curtain_intervals.Length;
        if (len == 0)
        {
            yield break;
        }

        do
        {
            int current = 0;
            while (current < len)
            {
                yield return new WaitForSeconds(curtain_intervals[current]);
                curtains[current].doCurtain();
                current++;
            }
        } while (repeat);
        
    }
}
