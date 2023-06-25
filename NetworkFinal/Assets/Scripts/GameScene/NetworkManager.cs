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
        DBLogin,
        DBSignUp,
        Quit,
        Update,
        Best
    }

    [Serializable]
    public class NetworkData
    {
        public Header head { get; set; }
        public byte[] data { get; set; }
    }

    public Queue<NetworkData> networkQueue = new Queue<NetworkData>();

    private string serverDomain;
    private int port;
    private Socket sock;
    private EndPoint srvEp;
    private NetworkStream networkStream;
    Thread rcvThread;

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
        /*try
        {
            TcpClient client = new TcpClient();
            client.Connect("127.0.0.1", port);
            networkStream = client.GetStream();
        }
        catch(Exception e)
        {
            Debug.LogError(e.Message);
        }*/
        IPAddress[] dnsToIp = Dns.GetHostAddresses(serverDomain);
        IPAddress srvIP = dnsToIp[0];
        sock = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        srvEp = new IPEndPoint(srvIP, port);
        /*Thread rcvThread = new Thread(RcvData);
        rcvThread.Start();*/
        // 절차 끝
    }

    private void OnDestroy()
    {
       /* rcvThread.Abort();*/
        sock.Close();
    }

    public void SendData(Header head, string uid, string data)
    {
        // 데이터를 Network클래스로 저장하고 xml로 직렬화 하는 절차
        NetworkData networkData = new NetworkData();
        networkData.head = head;
        string sendStr = $"{uid},{data}";
        networkData.data = Encoding.UTF8.GetBytes(sendStr);

        XmlSerializer xmlSerializer = new XmlSerializer(typeof(NetworkData));
        StringWriter stringWriter = new StringWriter();

        xmlSerializer.Serialize(stringWriter, networkData);
        string xmlString = stringWriter.ToString();
        // 절차 끝

        // 데이터를 byte로 서버에 보냄
        byte[] sendToServerData = Encoding.UTF8.GetBytes(xmlString);
        //networkStream.Write(sendToServerData, 0, sendToServerData.Length);
        sock.SendTo(sendToServerData, srvEp);

        // 데이터를 다시 받음
        byte[] rcvFromServerData = new byte[1024];
        int nRcvd = sock.ReceiveFrom(rcvFromServerData, ref srvEp);
        string rcvFromServerDataStr = Encoding.UTF8.GetString(rcvFromServerData, 0, nRcvd);
        StringReader stringReader = new StringReader(rcvFromServerDataStr);
        networkData = (NetworkData)xmlSerializer.Deserialize(stringReader);
        networkQueue.Enqueue(networkData);
    }

    #region Use Thread but Failed
    private void RcvData()
    {
        Debug.Log("RCV Thread Started");
        byte[] rcvData = new byte[1024];
        EndPoint ep = new IPEndPoint(IPAddress.Any, 0);
        while (true)
        {
            int nRcvd = sock.ReceiveFrom(rcvData, ref ep);
            string rcvDataStr = Encoding.UTF8.GetString(rcvData, 0, nRcvd);

            XmlSerializer xmlSerializer = new XmlSerializer(typeof(NetworkData));
            StringReader stringReader = new StringReader(rcvDataStr);

            NetworkData rcvNetworkData = (NetworkData)xmlSerializer.Deserialize(stringReader);
            networkQueue.Enqueue(rcvNetworkData);
        }
    }
    #endregion
}