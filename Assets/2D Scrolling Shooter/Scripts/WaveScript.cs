using UnityEngine;
using System.Collections;

//This script manages each wave of enemies
public class WaveScript : MonoBehaviour 
{
	GameObject[] waveShips;		//The ships that are in this wave


	void Awake()
	{
		//Get a reference to each ship (for efficiency)
        if (transform.Find("Enemies") != null)
        {
            waveShips = new GameObject[transform.Find("Enemies").childCount];
            for (int i = 0; i < transform.Find("Enemies").childCount; i++)
            {
                waveShips[i] = transform.Find("Enemies").GetChild(i).gameObject;
            }
        }
        else
        {
            waveShips = new GameObject[transform.childCount];
            for (int i = 0; i < transform.childCount; i++)
            {
                waveShips[i] = transform.GetChild(i).gameObject;
            }
        }
		
	}

	void OnEnable()
	{
		//When enabled, activate each child
		foreach(GameObject obj in waveShips)
		{
			obj.SetActive(true);
		}
	}

	public bool ShipsStillAlive()
	{
		//Check to see if any of the child ships are still active

		for(int i = 0; i < waveShips.Length; i++)
		{
			//If so, return true
			if(waveShips[i].activeSelf)
				return true;
		}
		//Otherwise, return false
		return false;
	}
}
