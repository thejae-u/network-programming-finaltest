using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObstacleController : MonoBehaviour
{
    private float curScaleX;
    private float curScaleY;

    private float speed;
    private float playerSpeed;
    private float scaleXSpeed;
    private float scaleYSpeed;

    private void Start()
    {
        GetSpeedFromGameManager();   
    }

    private void Update()
    {
        MoveControll();
        ScaleControll();
        PlayerMove();
        GetSpeedFromGameManager();
    }

    private void GetSpeedFromGameManager()
    {
        speed = GameManager.Instance.speed;
        playerSpeed = GameManager.Instance.speed;
        scaleXSpeed = GameManager.Instance.scaleXSpeed;
        scaleYSpeed = GameManager.Instance.scaleYSpeed;
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
        if (transform.localScale.x < 5)
            curScaleX = transform.localScale.x + scaleXSpeed * Time.deltaTime;
        if (transform.localScale.y < 5)
            curScaleY = transform.localScale.y + scaleYSpeed * Time.deltaTime;
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

        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (GameManager.Instance.IsBoost())
            {
                GameManager.Instance.Boost();
                // 스피드를 일시적 상승
            }
        }
    }


    private void OnTriggerEnter2D(Collider2D collision)
    {
        Destroy(gameObject);
    }
}
