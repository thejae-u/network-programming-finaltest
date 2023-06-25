/*using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Threading;
using System.Net.Sockets;
using System.Net;
using System.Xml.Serialization;
using System.IO;

namespace GameServer
{
    public enum Header
    {
        PlayerInput,
        GameData,
        GameOption,
        ETC
    }

    public class NetworkData
    {
        public Header head { get; set; }
        public byte[] data { get; set; }
    }

    public class Player
    {
        public string PlayerID { get; set; }
        public TcpClient Client { get; set; }
        public Thread ClientThread { get; set; }
        public bool IsConnected { get; set; }
        public IPEndPoint clientEp { get; set; }
    }

    class TcpServer
    {
        static private TcpListener listener;
        static private int maxPlayer = 2;
        static private Player[] players = new Player[maxPlayer];

        static private bool isRunning = true;

        static void Main(string[] args)
        {
            Console.WriteLine("Welcom Game Server");
            Thread server = new Thread(ServerStart);
            server.Start();
        }

        static void ServerStart(object obj)
        {
            listener = new TcpListener(IPAddress.Any, 55000);
            listener.Start();
            Console.WriteLine("Server Started");
            Thread waitPlayerThread = new Thread(WaitPlayer);
            waitPlayerThread.Start();
        }

        static void WaitPlayer()
        {
            while(isRunning)
            {
                Console.WriteLine("Client Wait...");
                TcpClient client;
                try
                {
                    client = listener.AcceptTcpClient();
                    NetworkStream clientStream = client.GetStream();
                    byte[] sendMessage = new byte[16];
                    Console.WriteLine($"Join Player");
                    Player player = CheckSlot();
                    if (player == null)
                    {
                        sendMessage = Encoding.UTF8.GetBytes("Room is Full");
                        clientStream.Write(sendMessage, 0, sendMessage.Length);
                        client.Close();
                        continue;
                    }

                    player.Client = client;
                    player.IsConnected = true;
                    Socket c = client.Client;
                    player.clientEp = (IPEndPoint)c.RemoteEndPoint;

                    Thread handler = new Thread(() => ClientHandler(player));
                    handler.Start();
                    player.ClientThread = handler;
                }
                catch(Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }
        }

        static Player CheckSlot()
        {
            for(int i = 0; i < maxPlayer; i++)
            {
                if(players[i] == null || !players[i].IsConnected)
                {
                    if (players[i] == null)
                        players[i] = new Player();
                    return players[i];
                }
            }
            return null;
        }

        static void ClientHandler(Player player)
        {
            NetworkStream clientStream = player.Client.GetStream();
            try
            {
                byte[] sendData;
                byte[] rcvData = new byte[1024];
                sendData = Encoding.UTF8.GetBytes("Connected");
                clientStream.Write(sendData, 0, sendData.Length);

                while (isRunning)
                {
                    NetworkData networkData;
                    int nRcvd = clientStream.Read(rcvData, 0, rcvData.Length);
                    string xmlRcvData = Encoding.UTF8.GetString(rcvData, 0, nRcvd);

                    XmlSerializer xmlSeralizer = new XmlSerializer(typeof(NetworkData));
                    StringReader stringReader = new StringReader(xmlRcvData);

                    networkData = (NetworkData)xmlSeralizer.Deserialize(stringReader);

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
                                case "LeftR":
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
                                    Console.WriteLine($"{player.clientEp} : StartGame");
                                    sendData = Encoding.UTF8.GetBytes("Start");
                                    break;
                                case "Hit":
                                    Console.WriteLine($"{player.clientEp} : HIT");
                                    sendData = Encoding.UTF8.GetBytes("Slow");
                                    break;
                                case "Boost":
                                    Console.WriteLine($"{player.clientEp} : BOOST");
                                    sendData = Encoding.UTF8.GetBytes("SpeedUp");
                                    break;
                                case "HitE":
                                case "BoostE":
                                    Console.WriteLine($"{player.clientEp} : State reset");
                                    sendData = Encoding.UTF8.GetBytes("ResetS");
                                    break;
                                case "Goal":
                                    Console.WriteLine($"{player.clientEp} : EndGame");
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
                            switch (rcvString)
                            {
                                case "Quit":
                                    player.IsConnected = false;
                                    clientStream.Close();
                                    break;
                            }
                            // 기타 다른 유형의 데이터 처리 ex) 데이터 저장 요청
                            break;
                        default:
                            Console.WriteLine($"{player.clientEp} : Wrong Header");
                            break;
                    }

                    clientStream.Write(sendData, 0, sendData.Length);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            finally
            {
                // 서버가 종료되면 클라이언트의 연결을 끊음
                clientStream.Close();
            }
        }
    }
}*/
