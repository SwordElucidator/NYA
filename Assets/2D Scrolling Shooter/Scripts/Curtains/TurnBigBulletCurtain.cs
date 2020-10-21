using System;
using UnityEngine;
using System.Collections;
using Random = UnityEngine.Random;

public class TurnBigBulletCurtain : Curtain
{
    public GameObject hugeBallBullet;
    public Transform[] hugeBallBulletEndPoints;
    public Transform[] hugeBallBulletSecondEndPoints;
    public Transform[] hugeBallBulletThirdEndPoints;
    public Transform[] hugeShotPositions;

    public float bulletInterval = 0.05F;
    public float waitInterval = 1F;
    public float arrivalTime = 0.5F;
    public float secondArrivalTime = 0.3F;
    public float thirdArrivalTime = 0.1F;

    public float finalTime = 2F;

    public Transform final;


    public override void doCurtain(Action<int> bossMoveFunction)
    {
        if (GetComponent<AudioSource>())
            GetComponent<AudioSource>().Play();
        Debug.Assert(hugeShotPositions.Length == hugeBallBulletEndPoints.Length);
        Debug.Assert(hugeShotPositions.Length == hugeBallBulletSecondEndPoints.Length);
        Debug.Assert(hugeShotPositions.Length == hugeBallBulletThirdEndPoints.Length);

        StartCoroutine(shoot(bossMoveFunction));
    }

    IEnumerator shoot(Action<int> bossMoveFunction)
    {

        var allBullets = new GameObject[hugeShotPositions.Length * 3];

        bossMoveFunction(0);
        
        for (int i = 0; i < hugeShotPositions.Length; i++)
        {
            yield return new WaitForSeconds(bulletInterval);
            //Get a pooled bullet
            GameObject obj = ObjectPool.current.GetObject(hugeBallBullet);
            //Set its position and rotation
            obj.transform.position = hugeShotPositions[i].position;

            Transform target = hugeBallBulletEndPoints[i];

            Vector3 vectorToTarget = target.position - hugeShotPositions[i].position;
            float angle = Mathf.Atan2(vectorToTarget.y, vectorToTarget.x) * Mathf.Rad2Deg - 90 + hugeShotPositions[i].rotation.z;
            Quaternion q = Quaternion.AngleAxis(angle, Vector3.forward);
            obj.transform.rotation = Quaternion.RotateTowards(obj.transform.rotation, q, 180);

            //Activate it
            obj.SetActive(true);

            obj.GetComponent<Bullet>().moveToByTime(target, arrivalTime);
            allBullets[i] = obj;
        }

        yield return new WaitForSeconds(waitInterval);
        
        bossMoveFunction(0);

        for (int i = 0; i < hugeShotPositions.Length; i++)
        {
            yield return new WaitForSeconds(bulletInterval);
            //Get a pooled bullet
            GameObject obj = ObjectPool.current.GetObject(hugeBallBullet);
            //Set its position and rotation
            obj.transform.position = hugeShotPositions[i].position;

            Transform target = hugeBallBulletSecondEndPoints[i];

            Vector3 vectorToTarget = target.position - hugeShotPositions[i].position;
            float angle = Mathf.Atan2(vectorToTarget.y, vectorToTarget.x) * Mathf.Rad2Deg - 90 + hugeShotPositions[i].rotation.z;
            Quaternion q = Quaternion.AngleAxis(angle, Vector3.forward);
            obj.transform.rotation = Quaternion.RotateTowards(obj.transform.rotation, q, 180);

            //Activate it
            obj.SetActive(true);

            obj.GetComponent<Bullet>().moveToByTime(target, secondArrivalTime);
            allBullets[hugeShotPositions.Length + i] = obj;
        }

        yield return new WaitForSeconds(waitInterval);
        
        bossMoveFunction(0);

        for (int i = 0; i < hugeShotPositions.Length; i++)
        {
            yield return new WaitForSeconds(bulletInterval);
            //Get a pooled bullet
            GameObject obj = ObjectPool.current.GetObject(hugeBallBullet);
            //Set its position and rotation
            obj.transform.position = hugeShotPositions[i].position;

            Transform target = hugeBallBulletThirdEndPoints[i];

            Vector3 vectorToTarget = target.position - hugeShotPositions[i].position;
            float angle = Mathf.Atan2(vectorToTarget.y, vectorToTarget.x) * Mathf.Rad2Deg - 90 + hugeShotPositions[i].rotation.z;
            Quaternion q = Quaternion.AngleAxis(angle, Vector3.forward);
            obj.transform.rotation = Quaternion.RotateTowards(obj.transform.rotation, q, 180);

            //Activate it
            obj.SetActive(true);

            obj.GetComponent<Bullet>().moveToByTime(target, thirdArrivalTime);
            allBullets[hugeShotPositions.Length * 2 + i] = obj;
        }
        
        yield return new WaitForSeconds(waitInterval);
        
        // FLY FLY
        var max = final.position.x;
        var min = - max;
        foreach (var bullet in allBullets)
        {
            var finalX = Random.value * (max - min) + min;
            bullet.GetComponent<Bullet>().moveToByTime(new Vector3(finalX, final.position.y, 0), finalTime);
        }
        
        yield return new WaitForSeconds(finalTime);
        
        
    }
}

