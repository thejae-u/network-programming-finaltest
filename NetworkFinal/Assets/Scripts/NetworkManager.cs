using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System;
using System.IO;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Xml.Serialization;
using System.Threading;
using System.Runtime.Serialization.Formatters.Binary;

public class NetworkManager : MonoBehaviour
{
    public enum Header
    {
        PlayerInput,
        GameData,
        GameOption,
        ETC
    }

    [Serializable]
    public class NetworkData
    {
        public Header head { get; set; }
        public byte[] data { get; set; }
    }

    private string serverDomain;
    private int port;
    private Socket sock;
    private EndPoint srvEp;

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
        // 데이터를 Network클래스로 저장하고 xml로 직렬화 하는 절차
        NetworkData networkData = new NetworkData();
        networkData.head = head;
        networkData.data = Encoding.UTF8.GetBytes(data);

        XmlSerializer xmlSerializer = new XmlSerializer(typeof(NetworkData));
        StringWriter stringWriter = new StringWriter();

        xmlSerializer.Serialize(stringWriter, networkData);
        string xmlString = stringWriter.ToString();
        // 절차 끝

        // 데이터를 byte로 서버에 보냄
        byte[] sendToServerData = Encoding.UTF8.GetBytes(xmlString);
        sock.SendTo(sendToServerData, srvEp);

        // 서버로 부터 데이터를 다시 받음
        byte[] rcvData = new byte[1024];
        int nRcvd = sock.ReceiveFrom(rcvData, ref srvEp);
        string rcvDataStr = Encoding.UTF8.GetString(rcvData, 0, nRcvd);

        return rcvDataStr;
    }
}