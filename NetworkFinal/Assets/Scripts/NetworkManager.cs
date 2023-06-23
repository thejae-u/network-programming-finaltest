using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System;
using System.IO;
using System.Text;
using System.Net;
using System.Net.Sockets;
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

    private string serverDomain;
    private int port;
    private Socket sock;
    private IPEndPoint srvEp;

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
        // 서버에 접속하기 위한 절차
        serverDomain = "jaeu.iptime.org";
        port = 55000;
        IPAddress[] dnsIpAddress = Dns.GetHostAddresses(serverDomain);
        IPAddress srvAddress = dnsIpAddress[0];
        sock = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        srvEp = new IPEndPoint(srvAddress, port);
        // 절차 끝
    }

    public string SendData(Header head, string data)
    {
        byte[] buf = Encoding.UTF8.GetBytes(data);
        switch (head)
        {
            case Header.PlayerInput:
                sock.SendTo(buf, srvEp);
                break;
            case Header.GameData:
                sock.SendTo(buf, srvEp);
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
