using UnityEngine;
using System.Collections;

public class Curtain : MonoBehaviour {

    //protected Transform[] shotPositions; // each curtain should have its own curtain positions
    // shot positions should also be set by the children since they may have different types of shot positions

    //additional bullets should be defined by override this class

    protected virtual void Awake()
    {
        //Get the fire points for future reference (this is for efficiency)
        //simple targets (etc. normal enemy)
        
    }


    //curtain will be accessed by calling doCurtain when needed.
    //if a curtain will be called several times, then make a IEm to do that in doCurtain
    public virtual void doCurtain()
    {

    }
}
