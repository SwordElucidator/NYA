using UnityEngine;
using System.Collections;
using Dreamteck.Splines;

public class Stage : MonoBehaviour {

    // Movement
    public SplineComputer[] computers;
    public float[] speeds;
    public float[] intervals; // intervals[0] is before the computers[0]
    public float stageInitialInterval = 2F;
    public float stageInterval = 2F;
    public float stageCurtainInterval = 2F;
    public bool repeat = true;

    // Curtain
    protected Curtain[] curtains;
    void OnEnable()
    {

        if (transform.Find("Curtains") != null)
        {
            curtains = new Curtain[transform.Find("Curtains").childCount];
            for (int i = 0; i < transform.Find("Curtains").childCount; i++)
            {
                curtains[i] = transform.Find("Curtains").GetChild(i).GetComponent<Curtain>();
            }
        }

        StartCoroutine(DoStageCurtain());
        // StartCoroutine("doStageMovement");

    }

    void OnDisable()
    {
        StopCoroutine(DoStageCurtain());
        // StopCoroutine("doStageMovement");
    }

    // IEnumerator doStageMovement()
    // {
    //     int len = intervals.Length;
    //     if (len == 0)
    //     {
    //         yield break;
    //     }
    //
    //     do
    //     {
    //         int current = 0;
    //         while (current < len)
    //         {
    //             yield return new WaitForSeconds(intervals[current]);
    //             // ALERT, we should know that transform's parent is Stages and it's parent is Enemy or Boss
    //             
    //         }
    //         yield return new WaitForSeconds(stageInterval);
    //     } while (repeat);
    // }
    
    
    private void DoBossMove(int index)
    {
        if (transform.parent.parent.GetComponent<SplineFollower>() == null)
        {
            transform.parent.parent.gameObject.AddComponent<SplineFollower>();
        }
        SplineFollower follower = transform.parent.parent.GetComponent<SplineFollower>();
        follower.computer = computers[index];
        follower.followSpeed = speeds[index];
        follower.autoFollow = true;
        follower.applyRotation = false;
        follower.applyScale = false;
        follower.applyPosition = true;
        follower.findStartPoint = false;
        follower.startPercent = 0;
        follower.averageResultVectors = false;
        follower.enabled = true;

        follower.Rebuild(true);
        follower.Restart();
        follower.SetPercent(0);
        follower.Move(1);
    }

    private IEnumerator DoStageCurtain()
    {
        yield return new WaitForSeconds(stageInitialInterval);
        do
        {
            int current = 0;
            while (current < curtains.Length)
            {
                curtains[current].doCurtain(DoBossMove);
                yield return new WaitForSeconds(curtains[current].totalSeconds);
                current++;
            }
            yield return new WaitForSeconds(stageCurtainInterval);
        } while (repeat);
        
    }
}
