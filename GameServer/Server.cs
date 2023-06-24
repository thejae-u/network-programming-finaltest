using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Xml.Serialization;
using System.Linq;
using System.Text;
using System.Threading;

using System.Net;
using System.Net.Sockets;
using MySql.Data.MySqlClient;

namespace GameServer
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
        public int seed { get; set; }
    }

    class Server
    {
        private static Socket servSock;
        private static IPEndPoint servEp;
        private static EndPoint clntEp;

        private static bool isRunning = true;

        static void Main(string[] args)
        {
            Thread serverThread = new Thread(StartServer);
            serverThread.Start();
            
            // 키 입력을 받으면 서버를 종료 시키기 위한 절차
            while (isRunning && !Console.KeyAvailable)
            {
                Thread.Sleep(100);
            }
            isRunning = false;
            serverThread.Join();
            Console.WriteLine("Server Stopped");
            Environment.Exit(1);
            // 절차 끝
        }

        private static void StartServer(object obj)
        {
            servSock = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            servEp = new IPEndPoint(IPAddress.Any, 55000);
            servSock.Bind(servEp);
            Thread recvThread = new Thread(ReceiveDataHandler);
            recvThread.Start();
            Console.WriteLine("Server Started...");
        }

        private static void ReceiveDataHandler(object obj)
        {
            Console.WriteLine("Wait for Client");
            clntEp = new IPEndPoint(IPAddress.None, 0);

            // 데이터 수신 절차 시작
            byte[] rcvData = new byte[1024];
            byte[] sendData = new byte[1024];
            while (isRunning)
            {
                int nRcvd = servSock.ReceiveFrom(rcvData, ref clntEp);
                string xmlRcvData = Encoding.UTF8.GetString(rcvData, 0, nRcvd);

                NetworkData networkData;

                XmlSerializer xmlSerializer = new XmlSerializer(typeof(NetworkData));
                StringReader stringReader = new StringReader(xmlRcvData);

                networkData = (NetworkData)xmlSerializer.Deserialize(stringReader);

                Header rcvHeader = networkData.head;
                string rcvString = Encoding.UTF8.GetString(networkData.data);

                switch (rcvHeader)
                {
                    case Header.PlayerInput:
                        switch (rcvString)
                        {
                            case "LeftS":
                                sendData = Encoding.UTF8.GetBytes("MoveL");
                                break;
                            case "RightS":
                                sendData = Encoding.UTF8.GetBytes("MoveR");
                                break;
                            case "LeftE":
                            case "RightE":
                                sendData = Encoding.UTF8.GetBytes("Stop");
                                break;
                        }
                        break;
                    case Header.GameData:
                        switch (rcvString)
                        {
                            case "Start":
                                Console.WriteLine($"{clntEp} : StartGame");
                                sendData = Encoding.UTF8.GetBytes("Start");
                                break;
                            case "Hit":
                                Console.WriteLine($"{clntEp} : HIT");
                                sendData = Encoding.UTF8.GetBytes("Slow");
                                break;
                            case "Boost":
                                Console.WriteLine($"{clntEp} : BOOST");
                                sendData = Encoding.UTF8.GetBytes("SpeedUp");
                                break;
                            case "HitE":
                            case "BoostE":
                                Console.WriteLine($"{clntEp} : State reset");
                                sendData = Encoding.UTF8.GetBytes("ResetS");
                                break;
                            case "Goal":
                                Console.WriteLine($"{clntEp} : EndGame");
                                sendData = Encoding.UTF8.GetBytes("EndSeq");
                                break;
                        }
                        break;

                    case Header.GameOption:
                        Random seedRandom = new Random();
                        int seed = seedRandom.Next();
                        sendData = Encoding.UTF8.GetBytes(seed.ToString());
                        break;
                    case Header.ETC:
                        // 기타 다른 유형의 데이터 처리 ex) 데이터 저장 요청
                        break;
                    default:
                        Console.WriteLine($"{clntEp} : Header Error Data");
                        break;
                }
                servSock.SendTo(sendData, clntEp);
                // 절차 끝

                // 키 입력을 받았을 때 쓰레드가 종료되기 위해 확인하는 절차
                if (Console.KeyAvailable)
                {
                    ConsoleKeyInfo keyInfo = Console.ReadKey(true);
                    if (keyInfo.Key == ConsoleKey.Escape)
                    {
                        isRunning = false;
                    }
                }
                //절차 종료
            }
        }
    }
}
