using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObstacleController : MonoBehaviour
{
    private float curScaleX;
    private float curScaleY;

    private float speed;
    private float playerSpeed;
    private float scaleSpeed;

    private void Start()
    {
        GetSpeedFromGameManager();   
    }

    private void Update()
    {
        GamePlayCheck();
        MoveControll();
        ScaleControll();
        PlayerMove();
        GetSpeedFromGameManager();
    }

    private void GamePlayCheck()
    {
        if (GameManager.Instance.IsGameOver)
            Destroy(gameObject);
    }

    private void GetSpeedFromGameManager()
    {
        speed = GameManager.Instance.CurObstacleSpeed;
        playerSpeed = GameManager.Instance.CurPlayerSpeed;
        scaleSpeed = GameManager.Instance.CurScaleSpeed;
    }

    private void MoveControll()
    {
        float yPos = transform.position.y - speed * Time.deltaTime;
        transform.position = new Vector2(transform.position.x, yPos);

        if (transform.position.y < -5)
            Destroy(gameObject);
    }

    private void ScaleControll()
    {
        curScaleX = transform.localScale.x + scaleSpeed * Time.deltaTime;
        curScaleY = transform.localScale.y + scaleSpeed * Time.deltaTime;
        transform.localScale = new Vector2(curScaleX, curScaleY);
    }

    private void PlayerMove()
    {
        if (Input.GetKey(KeyCode.LeftArrow))
        {
            float movePos = transform.position.x + playerSpeed * Time.deltaTime;
            transform.position = new Vector2(movePos, transform.position.y);
        }

        if (Input.GetKey(KeyCode.RightArrow))
        {
            float movePos = transform.position.x - playerSpeed * Time.deltaTime;
            transform.position = new Vector2(movePos, transform.position.y);
        }
    }


    private void OnTriggerEnter2D(Collider2D collision)
    {
        GameManager.Instance.SpeedDown();
        Destroy(gameObject);
    }
}
