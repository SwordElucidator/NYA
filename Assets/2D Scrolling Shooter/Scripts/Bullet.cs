﻿using UnityEngine;
using System.Collections;

//This script will handle the bullet adding itself back to the pool
public class Bullet : MonoBehaviour
{
	public float speed = 10;			//How fast the bullet moves
	public float lifeTime = 1;		//How long the bullet lives in seconds
	public int power = 1;           //Power of the bullet

    public float acceleration = 0F;

    //TODO angle change not working right now
    [HideInInspector]
    public float shotAngle = 0F;
    private float _shotAngle = 0F;

    [HideInInspector]
    public float bulletAngleRate = 0F;

    private Vector3 tmp = Vector3.zero;

    public float[] complexBulletAccList;
    public float[] complexBulletTimeList;


    

    void Update()
    {
        /*
        if (guideTime > 0)
        {
            float __shotAngle = SetTarget(targetTagName, guideTime);
            tmp.x = Mathf.Cos(__shotAngle * Mathf.Deg2Rad);
            tmp.y = Mathf.Sin(__shotAngle * Mathf.Deg2Rad);
            guideTime -= Time.deltaTime;
        }
        */
        //transform.position += (tmp * speed * Time.deltaTime);
        speed += acceleration * Time.deltaTime;
        GetComponent<Rigidbody2D>().velocity = transform.up.normalized * speed;


        shotAngle += bulletAngleRate * Time.deltaTime;

        if (_shotAngle != shotAngle)
        {
            transform.rotation = Quaternion.AngleAxis(shotAngle - 90.0f, Vector3.forward);
            _shotAngle = shotAngle;
        }
    }

    public void Calc()
    {
        tmp.x = Mathf.Cos(shotAngle * Mathf.Deg2Rad);
        tmp.y = Mathf.Sin(shotAngle * Mathf.Deg2Rad);
        transform.rotation = Quaternion.AngleAxis(shotAngle - 90.0f, Vector3.forward);
        _shotAngle = shotAngle;
    }

    /*
    public float SetTarget(string targetTagName, float guideTime)
    {
        this.targetTagName = targetTagName;
        this.guideTime = guideTime;
        var t = GameObject.FindWithTag(targetTagName);
        if (t)
        {
            shotAngle = GetAim(transform.position, t.transform.position);
        }
        else
        {
            Debug.LogError("Target Tag Not Found!!!");
        }
        return shotAngle;
    }*/

    private float GetAim(Vector2 p1, Vector2 p2)
    {
        float dx = p2.x - p1.x;
        float dy = p2.y - p1.y;
        float rad = Mathf.Atan2(dy, dx);
        return rad * Mathf.Rad2Deg;
    }
    

    void OnEnable ()
	{
        // go for complex changes
        StartCoroutine(complexChange());

		//Send the bullet "forward"
		GetComponent<Rigidbody2D>().velocity = transform.up.normalized * speed;
		//Invoke the Die method
		Invoke ("Die", lifeTime);
	}

	void OnDisable()
	{
		//Stop the Die method (in case something else put this bullet back in the pool)
		CancelInvoke ("Die");
	}

	void Die()
	{
		//Add the bullet back to the pool
		ObjectPool.current.PoolObject (gameObject);
	}

    IEnumerator complexChange()
    {
        int len = complexBulletAccList.Length;
        float last = 0F;
        int current = 0;
        while (current < len)
        {
            float period = complexBulletTimeList[current] - last;
            last = complexBulletTimeList[current];
            
            yield return new WaitForSeconds(period);
            acceleration = complexBulletAccList[current];
            current++;
        }
    }
}