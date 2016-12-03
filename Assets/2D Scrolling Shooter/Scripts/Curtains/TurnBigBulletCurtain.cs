using UnityEngine;
using System.Collections;

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

    public float finalSpeed = 5F;


    public override void doCurtain()
    {
        if (GetComponent<AudioSource>())
            GetComponent<AudioSource>().Play();
        Debug.Assert(hugeShotPositions.Length == hugeBallBulletEndPoints.Length);
        Debug.Assert(hugeShotPositions.Length == hugeBallBulletSecondEndPoints.Length);
        Debug.Assert(hugeShotPositions.Length == hugeBallBulletThirdEndPoints.Length);

        StartCoroutine(shoot());
    }

    IEnumerator shoot()
    {
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
        }

        yield return new WaitForSeconds(waitInterval);

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
        }

        yield return new WaitForSeconds(waitInterval);

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
        }
    }
}

