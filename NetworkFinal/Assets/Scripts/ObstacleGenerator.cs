using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObstacleGenerator : MonoBehaviour
{
    public GameObject obstaclePrefab;

    public float genTime;

    private float curTime;

    private void Start()
    {
        curTime = 0f;
    }

    private void Update()
    {
        GenObstacle();
    }

    private void GenObstacle()
    {
        curTime -= genTime * Time.deltaTime;

        if (curTime < 0)
        {
            curTime = genTime;
            float randomX = Random.Range(-10, 11);
            Instantiate(obstaclePrefab, new Vector2(randomX, 1.8f), Quaternion.identity);
        }
    }

}
