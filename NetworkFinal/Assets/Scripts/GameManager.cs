using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    private class InitTarget
    {
        private float scaleX;
        private float scaleY;
        private float posY;

        public float SX() { return scaleX; }
        public float SY() { return scaleY; }
        public float PY() { return posY; }

        public InitTarget(Transform tf)
        {
            scaleX = tf.localScale.x;
            scaleY = tf.localScale.y;
            posY = tf.position.y;
        }
    }
    
    private GameObject target;
    private InitTarget initTarget;

    private void Start()
    {
        target = GameObject.Find("Target");
        initTarget = new InitTarget(target.transform);
    }

    private void Update()
    {
        if (Input.GetKey(KeyCode.Space))
        {
            TargetCloser();       
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            Debug.Log("Hit R");
            ResetTarget();
        }
    }

    private void TargetCloser()
    {
        Vector2 targetScale = target.transform.localScale;
        float targetYPos = target.transform.position.y;
        targetScale.x += 0.1f;
        targetScale.y += 0.1f;
        if (targetYPos > 0)
            targetYPos -= 0.03f;
        target.transform.localScale = targetScale;
        target.transform.position = new Vector2(0, targetYPos);
    }

    private void ResetTarget()
    {
        target.transform.position = new Vector2(0, initTarget.PY());
        target.transform.localScale = new Vector2(initTarget.SX(), initTarget.SY());
    }
}
