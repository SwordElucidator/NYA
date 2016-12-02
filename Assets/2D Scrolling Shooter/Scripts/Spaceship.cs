using UnityEngine;
using System.Collections;

//This script is the base script for both Player and Enemy
//Ensure that the game object this is on has a rigidbody and animator
[RequireComponent(typeof(Rigidbody2D), typeof(Animator))]
public class Spaceship : MonoBehaviour
{
	public float speed;						//Ship's speed
	public float shotDelay;					//Delay between shots
	public GameObject bullet;				//The prefab of this ship's bullet
	public bool canShoot;					//Can this ship fire?
	public GameObject explosion;            //The prefab of this ship's explosion

    protected bool invincible = false;
    protected Transform[] shotPositions;	//Fire points on the ship
	protected Animator animator;
    //Reference to the ship's animator component

    public bool isEnemy = true;

    public bool aimed = false; // whether the ship is aiming the hero if the shot position is 180 degree

    public bool random_shoot = false; // whether this ship is doing a (-90, 90) degree random shooting from shot position
    
    //get the player when aimed
    private Player player;


    void Awake ()
	{
        //Get the fire points for future reference (this is for efficiency)

        // complex target
        if (transform.FindChild("ShotPositions") != null)
        {
            shotPositions = new Transform[transform.FindChild("ShotPositions").childCount];
            for (int i = 0; i < transform.FindChild("ShotPositions").childCount; i++)
            {
                shotPositions[i] = transform.FindChild("ShotPositions").GetChild(i);
            }
        }
        else
        {//simple targets (etc. normal enemy)
            shotPositions = new Transform[transform.childCount];
            for (int i = 0; i < transform.childCount; i++)
            {
                shotPositions[i] = transform.GetChild(i);
            }
        }
        
        //Get a reference to the animator component
        animator = GetComponent<Animator> ();
	}

	protected virtual void OnEnable()
	{
        if (aimed)
        {
            GameObject obj = GameObject.Find("Player");
            if (obj)
            {
                player = obj.GetComponent<Player>();
            }
            
        }
		//If the game is playing and the ship can shoot...
		if (canShoot && Manager.current.IsPlaying())
			//...Start it shooting
			StartCoroutine ("Shoot");
	}

	void OnDisable()
	{
		//If the ship was able to shoot and it became disabled...
		if(canShoot)
			//...Stop shooting
			StopCoroutine ("Shoot");
	}

	protected void Explode ()
	{
		//Get a pooled explosion object
		GameObject obj = ObjectPool.current.GetObject(explosion);
		//Set its position and rotation
		obj.transform.position = transform.position;
		obj.transform.rotation = transform.rotation;
		//Activate it
		obj.SetActive (true);
	}

	//Coroutine
	IEnumerator Shoot ()
	{
		//Loop indefinitely
		while(true)
		{
            //Wait for it to be time to fire another shot
            yield return new WaitForSeconds(shotDelay);
            //If there is an acompanying audio, play it
            if (GetComponent<AudioSource>())
				GetComponent<AudioSource>().Play ();
			//Loop through the fire points
			for(int i = 0; i < shotPositions.Length; i++)
			{
				//Get a pooled bullet
				GameObject obj = ObjectPool.current.GetObject(bullet);
				//Set its position and rotation
				obj.transform.position = shotPositions[i].position;
                
                if (aimed && isEnemy)
                {
                    if (player != null)
                    {
                        Vector3 vectorToTarget = player.transform.position - shotPositions[i].position;
                        float angle = Mathf.Atan2(vectorToTarget.y, vectorToTarget.x) * Mathf.Rad2Deg - 90 + shotPositions[i].rotation.z;
                        Quaternion q = Quaternion.AngleAxis(angle, Vector3.forward);
                        obj.transform.rotation = Quaternion.RotateTowards(obj.transform.rotation, q, 180);
                    }
                    else
                    {
                        obj.transform.rotation = shotPositions[i].rotation;
                    }

                }
                else if (random_shoot)
                {
                    obj.transform.rotation = shotPositions[i].rotation;
                    float rand = Random.Range(-90F, 90F);
                    obj.transform.rotation *= Quaternion.Euler(0, 0, rand);
                }
                else
                {
                    obj.transform.rotation = shotPositions[i].rotation;
                }
                //obj.transform.rotation = shotPositions[i].rotation;

                //Activate it
                obj.SetActive(true);
			}
			
		}
	}
}