using System;
using System.Collections.Generic;
using System.Collections;
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
        public int seed { get; set; }
    }

    public class Player
    {
        public string uid { get; set; }
        public string distance { get; set; }
        public string time { get; set; }
        public bool isPlaying { get; set; }
    }

    class Server
    {
        private static Socket servSock;
        private static IPEndPoint servEp;
        private static EndPoint clntEp;

        private static bool isRunning = true;

        private static List<Player> players = new List<Player>();

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

            string strConn = "Server = localhost;Database = ckgameboat; Uid = root;Pwd = root";

            while (isRunning)
            {
                MySqlConnection conn = new MySqlConnection(strConn);

                int nRcvd = servSock.ReceiveFrom(rcvData, ref clntEp);
                string xmlRcvData = Encoding.UTF8.GetString(rcvData, 0, nRcvd);

                NetworkData networkData;

                XmlSerializer xmlSerializer = new XmlSerializer(typeof(NetworkData));
                StringReader stringReader = new StringReader(xmlRcvData);

                networkData = (NetworkData)xmlSerializer.Deserialize(stringReader);

                Header rcvHeader = networkData.head;
                string rcvString = Encoding.UTF8.GetString(networkData.data);

                // DB를 위한 변수
                string[] datas;
                string uid;
                string upw;
                string query;

                switch (rcvHeader)
                {
                    case Header.PlayerInput:
                        datas = rcvString.Split(',');
                        switch (datas[1])
                        {
                            case "LeftS":
                                networkData.data = Encoding.UTF8.GetBytes("MoveL");
                                break;
                            case "RightS":
                                networkData.data = Encoding.UTF8.GetBytes("MoveR");
                                break;
                            case "LeftE":
                            case "RightE":
                                networkData.data = Encoding.UTF8.GetBytes("Stop");
                                break;
                        }
                        break;
                    case Header.GameData:
                        datas = rcvString.Split(',');
                        switch (datas[1])
                        {
                            case "Start":
                                Console.WriteLine($"{clntEp} : StartGame");
                                foreach(var p in players)
                                {
                                    if (p.uid == datas[0])
                                        p.isPlaying = true;
                                }
                                networkData.data = Encoding.UTF8.GetBytes("Start");
                                break;
                            case "Hit":
                                Console.WriteLine($"{clntEp} : HIT");
                                networkData.data = Encoding.UTF8.GetBytes("Slow");
                                break;
                            case "Boost":
                                Console.WriteLine($"{clntEp} : BOOST");
                                networkData.data = Encoding.UTF8.GetBytes("SpeedUp");
                                break;
                            case "HitE":
                            case "BoostE":
                                Console.WriteLine($"{clntEp} : State reset");
                                networkData.data = Encoding.UTF8.GetBytes("ResetS");
                                break;
                            case "Goal":
                                Console.WriteLine($"{clntEp} : EndGame (score : {float.Parse(datas[2]):F3})");
                                foreach(var p in players)
                                {
                                    if (p.uid == datas[0])
                                        p.isPlaying = false;
                                }
                                query = "SELECT score FROM userinfo WHERE uid = @userID";
                                try
                                {
                                    conn.Open();
                                    MySqlCommand cmd = new MySqlCommand(query, conn);
                                    cmd.Parameters.AddWithValue("@userID", datas[0]);
                                    MySqlDataReader rd = cmd.ExecuteReader();
                                    while (rd.Read())
                                    {
                                        float newTime = float.Parse($"{datas[2]:F3}");
                                        float savedTime = float.Parse(rd.GetString("score"));
                                        bool isUpdated = false;
                                        if (savedTime > newTime)
                                        {
                                            isUpdated = true;
                                        }

                                        if (isUpdated)
                                        {
                                            rd.Close();
                                            query = "UPDATE userinfo SET score = @newScore WHERE uid = @userID";
                                            cmd = new MySqlCommand(query, conn);
                                            cmd.Parameters.AddWithValue("@newScore", newTime);
                                            cmd.Parameters.AddWithValue("@userID", datas[0]);
                                            cmd.ExecuteNonQuery();
                                        }
                                    }
                                }
                                catch(Exception e)
                                {
                                }
                                finally
                                {
                                    conn.Close();
                                }
                                break;
                            case "Distance":
                                foreach(var p in players)
                                {
                                    if(p.uid == datas[0] && p.isPlaying)
                                    {
                                        p.distance = datas[2].ToString();
                                        p.time = datas[3].ToString();
                                    }
                                }
                                break;
                        }
                        break;

                    case Header.GameOption:
                        Random seedRandom = new Random();
                        int seed = seedRandom.Next();
                        networkData.data = Encoding.UTF8.GetBytes(seed.ToString());
                        break;
                    case Header.DBLogin:
                        datas = rcvString.Split(',');
                        uid = datas[0];
                        upw = datas[1];
                        conn.Open();
                        query = $"SELECT * FROM userinfo WHERE uid = @userID";
                        try
                        {
                            MySqlCommand cmd = new MySqlCommand(query, conn);
                            cmd.Parameters.AddWithValue("@userID", uid);
                            MySqlDataReader rd = cmd.ExecuteReader();
                            if (rd.HasRows)
                            {
                                rd.Close();
                                query = $"SELECT uid FROM userinfo WHERE pw = SHA2(@userPW, 256)";
                                cmd = new MySqlCommand(query, conn);
                                cmd.Parameters.AddWithValue("@userPW", upw);
                                rd = cmd.ExecuteReader();
                                while (rd.Read())
                                {
                                    if (rd.GetString("uid") == uid)
                                    {
                                        Console.WriteLine($"{clntEp} : Login (UID - {uid})");
                                        networkData.data = Encoding.UTF8.GetBytes("Success");
                                        Player player = new Player();
                                        player.distance = "100";
                                        player.uid = uid;
                                        player.time = "0";
                                        player.isPlaying = false;
                                        players.Add(player);
                                    }
                                }
                                rd.Close();
                            }
                            else
                            {
                                Console.WriteLine($"{clntEp} : Login Failed");
                                networkData.data = Encoding.UTF8.GetBytes("Fail");
                            }
                        }
                        catch(Exception e) 
                        {
                            Console.WriteLine(e.Message);
                        }
                        finally
                        {
                            conn.Close();
                        }
                        break;
                    case Header.DBSignUp:
                        try
                        {
                            datas = rcvString.Split(',');
                            conn.Open();
                            query = "INSERT INTO userinfo VALUES(@userID, sha2(@userPW, 256), 0)";
                            MySqlCommand cmd = new MySqlCommand(query, conn);
                            cmd.Parameters.AddWithValue("@userID", datas[0]);
                            cmd.Parameters.AddWithValue("@userPW", datas[1]);
                            cmd.ExecuteNonQuery();
                            networkData.data = Encoding.UTF8.GetBytes("Success");
                            Console.WriteLine($"{clntEp} : SIGN UP REQUERST SUCCESS");
                        }
                        catch (Exception e)
                        {
                            networkData.data = Encoding.UTF8.GetBytes("Fail");
                            Console.WriteLine($"{clntEp} : SIGN UP REQUERST FAIL");
                            Console.WriteLine(e.Message);
                        }
                        finally
                        {
                            conn.Close();
                        }

                        break;
                    case Header.Quit:
                        datas = rcvString.Split(',');
                        Console.WriteLine($"{clntEp} : quit (UID - {datas[0]})");
                        foreach(var player in players)
                        {
                            if(player.uid == datas[0])
                            {
                                players.RemoveAt(players.IndexOf(player));
                                break;
                            }
                        }
                        break;

                    case Header.Update:
                        datas = rcvString.Split(',');
                        switch (datas[1])
                        {
                            case "Player":
                                string userInfo = null;
                                foreach (var p in players)
                                {
                                    if (p.isPlaying)
                                    {
                                        string uidInPlayers = p.uid;
                                        string distanceInPlayers = p.distance;
                                        string timeInPlayers = p.time;
                                        userInfo += $"{uidInPlayers} {distanceInPlayers:F3} {timeInPlayers:F3},";
                                    }
                                }
                                if (userInfo != null)
                                {
                                    networkData.data = Encoding.UTF8.GetBytes(userInfo);
                                }
                                else
                                {
                                    networkData.data = Encoding.UTF8.GetBytes("");
                                }
                                break;
                        }
                        break;
                    case Header.Best:
                        datas = rcvString.Split(',');
                        query = "SELECT score FROM userinfo WHERE uid = @userID";
                        try
                        {
                            conn.Open();
                            MySqlCommand cmd = new MySqlCommand(query, conn);
                            cmd.Parameters.AddWithValue("@userID", datas[0]);
                            MySqlDataReader rd = cmd.ExecuteReader();
                            while (rd.Read())
                            {
                                networkData.data = Encoding.UTF8.GetBytes(rd.GetString("score"));
                            }
                        }
                        catch (Exception e)
                        {
                        }
                        finally
                        {
                            conn.Close();
                        }
                        break;
                    default:
                        Console.WriteLine($"{clntEp} : Header Error Data");
                        break;
                }
                // 다시 보내기 위한 직렬화
                StringWriter stringWriter = new StringWriter();
                xmlSerializer.Serialize(stringWriter, networkData);
                string xmlString = stringWriter.ToString();
                // 직렬화 끝

                sendData = Encoding.UTF8.GetBytes(xmlString);
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
