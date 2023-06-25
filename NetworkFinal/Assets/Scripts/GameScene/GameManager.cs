using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Text;

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
    public TMP_Text bestScore;

    public GameObject scrollView;

    private static GameManager instance;

    // 유저의 정보를 담아두는 변수
    private USERINFO uinfo;

    // 바다의 무한 스크롤을 위한 변수
    private MeshRenderer groundRender;
    private float groundOffset;

    // 총 걸린 시간
    private float mainTime;

    // 저장된 최고 점수 (시간)
    private float bestTime;

    // 장애물에 부딪혔을 때 속도가 낮아진 상태인지를 저장하는 변수
    private bool isSlow;

    private bool isDistanceUpdateToServer = false;

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
        uinfo = GameObject.Find("USERINFO").GetComponent<USERINFO>();
        gameOverPanel.SetActive(false);
        scrollView.SetActive(true);
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
        if (IsGameOver)
        {
            return;
        }
        if (!IsStarted)
        {
            StartWait();
            return;
        }

        if (IsStarted)
        {
            NetworkDequeue();
            TimeCheck();
            GroundRepeat();
            Distance();
            PlayerInput();
        }
    }
    
    // 서버로부터 받은 정보를 하나씩 처리
    private void NetworkDequeue()
    {
        if (NetworkManager.Instance.networkQueue.Count != 0)
        {
            NetworkManager.NetworkData rcvdData = NetworkManager.Instance.networkQueue.Dequeue();
            string rcvdDataString = Encoding.UTF8.GetString(rcvdData.data);
            switch (rcvdData.head)
            {
                case NetworkManager.Header.PlayerInput:
                    switch (rcvdDataString)
                    {
                        case "MoveL":
                            MoveDirect = rcvdDataString;
                            break;
                        case "MoveR":
                            MoveDirect = rcvdDataString;
                            break;
                        case "Stop":
                            MoveDirect = "";
                            break;
                        case "Boost":
                            Boost();
                            break;
                    }
                    break;
                case NetworkManager.Header.Best:
                    Debug.Log($"{rcvdDataString}");
                    break;
            }
        }
    }

    private void StartWait()
    {
        distanceText.text = "Click Space to Start!";
        if (Input.GetKeyDown(KeyCode.Space) && !IsStarted)
        {
            NetworkManager.Instance.SendData(NetworkManager.Header.GameData, uinfo.Uid, "Start");
            IsStarted = true;
        }
    }
    
    private void Distance()
    {
        if (distance < 0 && !IsGameOver)
        {
            distance = 0;
            IsGameOver = true;
            NetworkManager.Instance.SendData(NetworkManager.Header.GameData, uinfo.Uid, $"Goal,{mainTime}");
            GameOverSeq();
            return;
        }

        if (!isDistanceUpdateToServer && distance != 0)
        {
            StartCoroutine(WaitUpdateDistance());
        }
        distance -= CurPlayerSpeed * Time.deltaTime;
        distanceText.text = $"Distance : {distance:F3}M";
    }

    private IEnumerator WaitUpdateDistance()
    {
        isDistanceUpdateToServer = true;
        NetworkManager.Instance.SendData(NetworkManager.Header.GameData, uinfo.Uid, $"Distance,{distance:F3},{mainTime:F3}");
        yield return new WaitForSeconds(0.1f);
        isDistanceUpdateToServer = false;
    }

    private void TimeCheck()
    {
        mainTime += Time.deltaTime;
        timeText.text = $"Time : {mainTime:F3}s";
    }

    private void GameOverSeq()
    {
        NetworkManager.Instance.SendData(NetworkManager.Header.Best, uinfo.Uid, "");
        StartCoroutine(WaitEndTime());
    }

    private IEnumerator WaitEndTime()
    {
        yield return new WaitForSeconds(0.5f);
        gameOverPanel.SetActive(true);
        scrollView.SetActive(false);
        timeText.text = "";
        distanceText.text = "";
        boostImage.color = new Color(0, 0, 0, 0);
        timeEndText.text = $"Time : {mainTime.ToString("F3")}";
    }

    public void OnRestartButtonClick()
    {
        Destroy(gameObject);
        SceneManager.LoadScene("GameScene");
    }

    public void OnQuitButtonClick()
    {
        NetworkManager.Instance.SendData(NetworkManager.Header.Quit, uinfo.Uid, "Quit");
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
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
            NetworkManager.Instance.SendData(NetworkManager.Header.GameData, uinfo.Uid, "Hit");
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

    
    private void PlayerInput()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (IsBoostAva)
            {
                NetworkManager.Instance.SendData(NetworkManager.Header.GameData, uinfo.Uid, "Boost");
                boostImage.color = new Color(0, 0, 0, 0);
                Boost();
            }
        }

        PlayerMove();
    }
    private void PlayerMove()
    {
        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            NetworkManager.Instance.SendData(NetworkManager.Header.PlayerInput, uinfo.Uid, "LeftS");
        }

        if (Input.GetKeyUp(KeyCode.LeftArrow))
        {
            NetworkManager.Instance.SendData(NetworkManager.Header.PlayerInput, uinfo.Uid, "LeftE");
        }

        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            NetworkManager.Instance.SendData(NetworkManager.Header.PlayerInput, uinfo.Uid, "RightS");
        }

        if (Input.GetKeyUp(KeyCode.RightArrow))
        {
            NetworkManager.Instance.SendData(NetworkManager.Header.PlayerInput, uinfo.Uid, "RightE");
        }
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
        yield return new WaitForSeconds(10.0f);
        if (!IsGameOver)
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
