using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

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

    public Image boostImage;

    public GameObject gameOverPanel;
    public TMP_Text timeEndText;

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
    public bool IsStarted { get; private set; }
    public bool IsGameOver { get; private set; }

    public string MoveDirect { get; private set; }

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
        gameOverPanel.SetActive(false);
        boostImage.color = new Color(255, 255, 255, 255);
        IsBoost = false;
        IsBoostAva = true;
        IsStarted = false;
        distance = 100;
        mainTime = 0;
        InitSpeed();
    }

    private void Update()
    {
        if (IsGameOver) return;
        if (!IsStarted)
        {
            StartWait();
            return;
        }

        if (IsStarted)
        {
            TimeCheck();
            GroundRepeat();
            Distance();
            PlayerInput();
        }
    }

    private void StartWait()
    {
        distanceText.text = "Click Space to Start!";
        if (Input.GetKeyDown(KeyCode.Space) && !IsStarted)
        {
            NetworkManager.Instance.SendData(NetworkManager.Header.GameData, "Start");
            IsStarted = true;
        }
    }
    
    private void Distance()
    {
        if (distance < 0 && !IsGameOver)
        {
            distance = 0;
            distanceText.text = "GOAL";
            IsGameOver = true;
            NetworkManager.Instance.SendData(NetworkManager.Header.GameData, "Goal");
            GameOverSeq();
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

    private void GameOverSeq()
    {
        gameOverPanel.SetActive(true);
        timeText.text = "";
        distanceText.text = "";
        boostImage.color = new Color(0, 0, 0, 0);
        timeEndText.text = mainTime.ToString("F3");
    }

    public void OnRestartButtonClick()
    {
        Destroy(gameObject);
        SceneManager.LoadScene("GameScene");
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
            NetworkManager.Instance.SendData(NetworkManager.Header.GameData, "Hit");
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

    private void PlayerMove()
    {
        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            MoveDirect = NetworkManager.Instance.SendData(NetworkManager.Header.PlayerInput, "LeftS");
        }

        if (Input.GetKeyUp(KeyCode.LeftArrow))
        {
            MoveDirect = NetworkManager.Instance.SendData(NetworkManager.Header.PlayerInput, "LeftE");
        }

        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            MoveDirect = NetworkManager.Instance.SendData(NetworkManager.Header.PlayerInput, "RightS");
        }

        if (Input.GetKeyUp(KeyCode.RightArrow))
        {
            MoveDirect = NetworkManager.Instance.SendData(NetworkManager.Header.PlayerInput, "RightE");
        }
    }
    private void PlayerInput()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (IsBoostAva)
            {
                NetworkManager.Instance.SendData(NetworkManager.Header.GameData, "Boost");
                boostImage.color = new Color(0, 0, 0, 0);
                Boost();
            }
        }

        PlayerMove();
    }

    #region Boost

    private void Boost()
    {
        IsBoost = true;
        IsBoostAva = false;
        StartCoroutine(BoostWait());
    }

    private IEnumerator BoostWait()
    {
        StartCoroutine(Boosting());
        yield return new WaitForSeconds(5.0f);
        if (IsGameOver)
            boostImage.color = new Color(255, 255, 255, 255);
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
