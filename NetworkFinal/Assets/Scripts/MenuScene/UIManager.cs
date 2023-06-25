using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Text;
using System.Xml.Serialization;
using System;

public class UIManager : MonoBehaviour
{
    public TMP_Text stateText;
    public TMP_Text stateTextIn;

    public TMP_InputField id;
    public TMP_InputField pw;
    public TMP_InputField sId;
    public TMP_InputField sPw;

    public GameObject panel;

    private string idStr = null;
    private string pwStr = null;

    Socket sock = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
    string serverDomain = "jaeu.iptime.org";
    EndPoint srvEp;
    int port = 55000;
    byte[] sendByte = new byte[1024];

    private USERINFO uinfo;
    private NetworkManager.NetworkData networkData;

    private void Start()
    {
        uinfo = GameObject.Find("USERINFO").GetComponent<USERINFO>();
        IPAddress[] dnsToIp = Dns.GetHostAddresses(serverDomain);
        IPAddress srvIP = dnsToIp[0];
        sock = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        srvEp = new IPEndPoint(srvIP, port);

        stateText.text = "";
        stateTextIn.text = "";
    }

    public void OnStartButtonClick()
    {
        string result = SendData(NetworkManager.Header.DBLogin);
        switch (result)
        {
            case "Success":
                uinfo.LoginSuccess(idStr);
                SceneManager.LoadScene("GameScene");
                break;
            case "Fail":
                stateText.text = "Wrong";
                break;
            case "":
                break;
        }
    }

    public void OnSingUpButtonClick()
    {
        panel.SetActive(true);
        id.GetComponent<TMP_InputField>().text = "";
        pw.GetComponent<TMP_InputField>().text = "";
    }

    public void OnSignUpButtonClickIn()
    {
        string result = SendData(NetworkManager.Header.DBSignUp);
        switch (result)
        {
            case "Success":
                panel.SetActive(false);
                stateText.text = "Login Please";
                break;
            case "Fail":
                stateTextIn.text = "Failed : Id Already Exist";
                break;
            case "":
                break;
        }
    }

    public void OnBackButtonClick()
    {
        panel.SetActive(false);
    }

    private string SendData(NetworkManager.Header head)
    {
        idStr = id.GetComponent<TMP_InputField>().text;
        pwStr = pw.GetComponent<TMP_InputField>().text;

        if (idStr.Length == 0 || pwStr.Length == 0)
        {
            idStr = sId.GetComponent<TMP_InputField>().text;
            pwStr = sPw.GetComponent<TMP_InputField>().text;
            Debug.Log(pwStr);
            string[] strs = { idStr, pwStr };
            return Send(head, strs);
        }
        else
        {
            string[] strs = { idStr, pwStr };
            return Send(head, strs);
        }
    }

    private string Send(NetworkManager.Header head, string[] strs)
    {
        try { 
            networkData = new NetworkManager.NetworkData();
            networkData.head = head;
            string data = string.Join(",", strs);
            Debug.Log(data);
            networkData.data = Encoding.UTF8.GetBytes(data);

            XmlSerializer xmlSerializer = new XmlSerializer(typeof(NetworkManager.NetworkData));
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

            StringReader stringReader = new StringReader(rcvDataStr);

            networkData = (NetworkManager.NetworkData)xmlSerializer.Deserialize(stringReader);

            rcvDataStr = Encoding.UTF8.GetString(networkData.data);

            return rcvDataStr;
        }
        catch (Exception e)
        {
            Debug.LogError(e.Message);
        }
        return "";
    }
}
