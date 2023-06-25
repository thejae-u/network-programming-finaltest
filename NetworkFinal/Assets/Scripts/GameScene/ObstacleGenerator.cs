using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;

public class ObstacleGenerator : MonoBehaviour
{
    public GameObject obstaclePrefab;

    public float genTime;

    private USERINFO uinfo;

    private float curTime;

    private void Start()
    {
        uinfo = GameObject.Find("USERINFO").GetComponent<USERINFO>();
        curTime = 0f;
    }

    private void Update()
    {
        if (!GameManager.Instance.IsGameOver && GameManager.Instance.IsStarted)
            GenObstacle();
    }

    private void GenObstacle()
    {
        curTime -= genTime * Time.deltaTime;

        if (curTime < 0)
        {
            if (!GameManager.Instance.IsBoost)
                curTime = genTime;
            else
                curTime = genTime / 2;

            int seed = 0;
            NetworkManager.Instance.SendData(NetworkManager.Header.GameOption, uinfo.Uid, "");
            foreach(var networkData in NetworkManager.Instance.networkQueue)
            {
                if(networkData.head == NetworkManager.Header.GameOption)
                {
                    string seedStr = Encoding.UTF8.GetString(networkData.data);
                    seed = int.Parse(seedStr);
                }
            }
            Random.InitState(seed);
            float randomX = Random.Range(-8, 9);
            Instantiate(obstaclePrefab, new Vector2(randomX, 1.8f), Quaternion.identity);
        }
    }

}
