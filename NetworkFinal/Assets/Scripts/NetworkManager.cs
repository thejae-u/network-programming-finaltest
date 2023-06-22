using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System;
using System.IO;
using System.Text;
using System.Net;
using System.Threading;
using System.Runtime.Serialization.Formatters.Binary;

public class NetworkManager : MonoBehaviour
{
    public enum Header
    {
        PlayerInput,
        GameOption,
        GameData,
        ETC
    }

    private static NetworkManager instance;

    public static NetworkManager Instance
    {
        get
        {
            if(instance == null)
            {
                return null;
            }
            return instance;
        }
    }
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

    private void Start()
    {
       
    }

    public string SendData(string data)
    {
        string[] splitData = data.Split(',');
        Header dataHeader = (Header)int.Parse(splitData[0]);
        switch (dataHeader)
        {
            case Header.PlayerInput:
                break;
            case Header.GameOption:
                break;
            case Header.GameData:
                break;
            case Header.ETC:
                break;
            default:
                Debug.LogError("Network Header Error");
                break;
        }
        return "";
    }
}
