using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class GameManager : MonoBehaviour
{
    public float obstacleSpeed;
    public float playerSpeed;

    // 장애물이 커지는 속도
    public float scaleSpeed;

    public float boostSpeed;
    public float boostTime;

    public float downSpeed;

    // 바다의 무한 스크롤의 속도
    public float groundSpeed;

    public TMP_Text distanceText;
    public TMP_Text timeText;

    private static GameManager instance;

    // 바다의 무한 스크롤을 위한 변수
    private MeshRenderer groundRender;
    private float groundOffset;

    // 총 걸린 시간
    private float mainTime;

    // 장애물에 부딪혔을 때 속도가 낮아진 상태인지를 저장하는 변수
    private bool isSlow;

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
        if (IsGameOver)
        {
            return;
        }

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
        distance -= CurPlayerSpeed * Time.deltaTime;
        distanceText.text = $"Distance : {distance:F3}M";
    }

    private void TimeCheck()
    {
        mainTime += Time.deltaTime;
        timeText.text = $"Time : {mainTime:F3}s";
    }

    // 바다의 무한 스크롤을 위한 메소드
    private void GroundRepeat()
    {
        groundOffset += groundSpeed * Time.deltaTime;
        groundRender.material.mainTextureOffset = new Vector2(0, groundOffset);
    }

    
    // 플레이어가 장애물에 부딪히면 호출되는 메소드
    public void SpeedDown()
    {
        if (!isSlow)
        {
            isSlow = true;
            CurPlayerSpeed /= downSpeed;
            CurObstacleSpeed /= downSpeed;
            CurScaleSpeed /= downSpeed;
            StartCoroutine(SpeedDownTime());
        }
    }

    private IEnumerator SpeedDownTime()
    {
        yield return new WaitForSeconds(3.0f);
        isSlow = false;
        ResetSpeed();
    }

    #region Boost
    private void PlayerInput()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            NetworkManager.Instance.SendData($"{NetworkManager.Header.GameData},Boost");
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
