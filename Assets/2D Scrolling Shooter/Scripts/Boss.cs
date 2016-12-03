using UnityEngine;
using System.Collections;

public class Boss : Enemy
{
    protected Stage[] stages;
    public int[] hplines; //recommand first element equals to the max hp
    private int toReachHpLineIndex = 0;

    public float preStageTime = 1F;
    // Use this for initialization
    protected override void OnEnable()
    {
        //Call parent's OnEnable method
        base.OnEnable();

        if (hplines == null || hplines.Length == 0)
        {
            hplines = new int[1];
            hplines[0] = hp - 1;
        }

        if (transform.FindChild("Stages") != null)
        {
            stages = new Stage[transform.FindChild("Stages").childCount];
            for (int i = 0; i < transform.FindChild("Stages").childCount; i++)
            {
                stages[i] = transform.FindChild("Stages").GetChild(i).GetComponent<Stage>();
            }
        }

        invincible = true;

        StartCoroutine(preStage());


    }

    IEnumerator preStage()
    {
        yield return new WaitForSeconds(preStageTime);
        speed = 0;
        GetComponent<Rigidbody2D>().velocity = (transform.up * -1) * 0;
        invincible = false;
        headstrong = true;
        // TODO more details later
    }

    protected override void OnTriggerEnter2D(Collider2D c)
    {
        base.OnTriggerEnter2D(c);
        if (invincible)
        {
            return;
        }
        if (toReachHpLineIndex >= hplines.Length)
        {
            return;
        }
        if (currentHP <= hplines[toReachHpLineIndex])
        {
            if (toReachHpLineIndex > 0)
            {
                stages[toReachHpLineIndex - 1].gameObject.SetActive(false);
            }
            stages[toReachHpLineIndex].gameObject.SetActive(true);
            toReachHpLineIndex++;
        }
    }
}
