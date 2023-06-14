using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public float speed;
    public float playerSpeed;
    public float scaleXSpeed;
    public float scaleYSpeed;

    private static GameManager instance;

    private bool isBoost;

    private float distance;

    private void Awake()
    {
        if(instance == null)
        {
            instance = this;

            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public static GameManager Instance
    {
        get
        {
            if(instance== null)
            {
                return null;
            }
            return instance;
        }
    }

    private void Start()
    {
        distance = 2000;
    }

    private void Update()
    {
        distance -= playerSpeed * Time.deltaTime;
    }

    public bool IsBoost()
    {
        return isBoost;
    }

    public void Boost()
    {
        isBoost = true;
        StartCoroutine(Boosting());
    }

    private IEnumerator Boosting()
    {
        float savePlayerSpeed = playerSpeed;
        playerSpeed *= 2;
        yield return new WaitForSeconds(30.0f);
        playerSpeed = savePlayerSpeed;
        isBoost = false;
    }
}
