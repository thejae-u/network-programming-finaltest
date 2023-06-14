using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class GameManager : MonoBehaviour
{
    public float obstacleSpeed;
    public float playerSpeed;
    public float scaleSpeed;
    public float boostSpeed;
    public float boostTime;
    public TMP_Text distanceText;

    private static GameManager instance;

    public bool IsBoost { get; private set; }
    public bool IsBoostAva { get; private set; }

    private float distance;
    public float CurObstacleSpeed { get; private set; }
    public float CurScaleSpeed { get; private set; }
    public float CurPlayerSpeed { get; private set; }


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
            if (instance == null)
            {
                return null;
            }
            return instance;
        }
    }

    private void InitSpeed()
    {
        CurObstacleSpeed = obstacleSpeed;
        CurPlayerSpeed = playerSpeed;
        CurScaleSpeed = scaleSpeed;
    }
    
    private void Start()
    {
        IsBoost = false;
        IsBoostAva = true;
        distance = 1000;
        InitSpeed();
    }

    private void Update()
    {
        Distance();
        PlayerInput();
    }
    
    private void Distance()
    {
        distance -= playerSpeed * Time.deltaTime;
        distanceText.text = $"Distance : {distance}";
    }

    #region Boost
    private void PlayerInput()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (IsBoostAva)
                Boost();
        }
    }

    private void Boost()
    {
        IsBoost = true;
        IsBoostAva = false;
        StartCoroutine(BoostWait());
    }

    private IEnumerator BoostWait()
    {
        StartCoroutine(Boosting());
        yield return new WaitForSeconds(30.0f);
        IsBoostAva = true;
    }

    private IEnumerator Boosting()
    {
        SpeedUp(boostSpeed);
        yield return new WaitForSeconds(boostTime);
        IsBoost = false;
        ResetSpeed();
    }

    private void SpeedUp(float sp)
    {
        CurPlayerSpeed *= sp;
        CurObstacleSpeed *= sp;
        CurScaleSpeed *= sp;
    }

    private void ResetSpeed()
    {
        CurPlayerSpeed = playerSpeed;
        CurObstacleSpeed = obstacleSpeed;
        CurScaleSpeed = scaleSpeed;
    }

    #endregion
}
