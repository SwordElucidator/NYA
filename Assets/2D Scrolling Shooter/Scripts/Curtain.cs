using UnityEngine;
using System.Collections;

public class Curtain : MonoBehaviour {

    protected Transform[] shotPositions; // each curtain should have its own curtain positions


    void Awake()
    {
        //Get the fire points for future reference (this is for efficiency)
        //simple targets (etc. normal enemy)
        shotPositions = new Transform[transform.childCount];
        for (int i = 0; i < transform.childCount; i++)
        {
            shotPositions[i] = transform.GetChild(i);
        }
        
    }

    public void doCurtain()
    {

    }
}
