using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class USERINFO : MonoBehaviour
{
    public string Uid { get; private set; }

    public void LoginSuccess(string uid)
    {
        Uid = uid;
    }

    private void Start()
    {
        DontDestroyOnLoad(gameObject);
    }
}
