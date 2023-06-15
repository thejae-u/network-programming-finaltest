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
    public float groundSpeed;
    public TMP_Text distanceText;
    public TMP_Text timeText;

    private static GameManager instance;

    private MeshRenderer groundRender;
    private float groundOffset;

    private float mainTime;

    public bool IsBoost { get; private set; }
    public bool IsBoostAva { get; private set; }
    public bool IsGameOver { get; private set; }

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
        groundRender = GameObject.Find("Ground").GetComponent<MeshRenderer>();
        IsBoost = false;
        IsBoostAva = true;
        distance = 100;
        mainTime = 0;
        InitSpeed();
    }

    private void Update()
    {
        if (IsGameOver) return;
        TimeCheck();
        GroundRepeat();
        Distance();
        PlayerInput();
    }
    
    private void Distance()
    {
        if (distance < 0)
        {
            distance = 0;
            distanceText.text = "GOAL";
            IsGameOver = true;
            return;
        }
        distance -= playerSpeed * Time.deltaTime;
        distanceText.text = $"Distance : {distance}";
    }

    private void TimeCheck()
    {
        mainTime += Time.deltaTime;
        timeText.text = $"Time : {mainTime.ToString("F3")}s";
    }

    private void GroundRepeat()
    {
        groundOffset += groundSpeed * Time.deltaTime;
        groundRender.material.mainTextureOffset = new Vector2(0, groundOffset);
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
