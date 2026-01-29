using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

using System.Xml.Serialization;
using System.Text;

using System.Net;
using System.Net.Sockets;
using MySql.Data.MySqlClient;

using System.Collections.Concurrent;

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
        public required Header Head { get; set; }
        public required byte[] Data { get; set; }
        public required int Seed { get; set; }
    }

    public class Player
    {
        public required string Uid { get; set; }
        public required string Distance { get; set; }
        public required string Time { get; set; }
        public required bool IsPlaying { get; set; }
    }

    internal class Server
    {
        private Socket _servSock;
        private IPEndPoint _servEp;
        private EndPoint _clntEp;

        private Task? _recvTask;

        private ConcurrentDictionary<string, Player> _players = new();
        private DBManager _dbManager;

        private readonly object _lock = new();
        private bool _isRunning;

        public void SetFlag(bool value)
        {
            lock (_lock)
                _isRunning = value;
        }
        public bool GetFlag()
        {
            lock (_lock)
                return _isRunning;
        }

        public Server(ushort port)
        {
            _servSock = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

            _servEp = new IPEndPoint(IPAddress.Any, port);
            _clntEp = new IPEndPoint(IPAddress.None, 0);

            _dbManager = new("Server=localhost;Database=ckgameboat;Uid=root;Pwd=root");
        }

        public void Stop()
        {
            _recvTask?.Dispose();
        }

        public void StartServer()
        {
            _servSock.Bind(_servEp);

            SetFlag(true);
            _recvTask = Task.Run(ReceiveData);

            Console.WriteLine("Server Started...");
        }

        private void ReceiveData()
        {
            byte[] rcvData = new byte[1024];

            // 실제 오픈된 서버에서는 Env로 안전하게 관리

            Console.WriteLine("Recevie Stated...");

            while (GetFlag())
            {
                int nRcvd = _servSock.ReceiveFrom(rcvData, ref _clntEp);
                string xmlRcvData = Encoding.UTF8.GetString(rcvData, 0, nRcvd);


                XmlSerializer xmlSerializer = new XmlSerializer(typeof(NetworkData));
                StringReader stringReader = new StringReader(xmlRcvData);

                if (xmlSerializer is null)
                {
                    Console.WriteLine("Invalid situation: xmlSerializer is null");
                    SetFlag(false);
                    continue;
                }

                NetworkData? networkData;
                networkData = xmlSerializer.Deserialize(stringReader) as NetworkData;

                if (networkData is null)
                {
                    Console.WriteLine("Invalid Data received");
                    continue;
                }

                Header recvHeader = networkData.Head;
                if (recvHeader == Header.GameOption)
                {
                    Random seedRandom = new Random();
                    int seed = seedRandom.Next();
                    networkData.Data = Encoding.UTF8.GetBytes(seed.ToString());

                    SendMessage(networkData);
                    CheckServerState();
                    continue;
                }

                string rcvString = Encoding.UTF8.GetString(networkData.Data);
                if (!string.IsNullOrEmpty(rcvString))
                {
                    Console.WriteLine("Invalid network Data: rcvString is null");
                    continue;
                }

                string[] datas = rcvString.Split(',');
                if (datas.Length <= 0)
                {
                    Console.WriteLine("Invalid datas: length is invalid");
                    continue;
                }

                switch (recvHeader)
                {
                    case Header.PlayerInput:
                        {
                            var command = datas[1];
                            if (command == "LeftS") networkData.Data = Encoding.UTF8.GetBytes("MoveL");
                            else if (command == "RightS") networkData.Data = Encoding.UTF8.GetBytes("MoveR");
                            else if (command == "LeftE" || command == "RightE") networkData.Data = Encoding.UTF8.GetBytes("Stop");
                            else networkData.Data = Encoding.UTF8.GetBytes("");
                        }
                        
                        break;
                    case Header.GameData:
                        {
                            var command = datas[1];
                            if (command == "Start")
                            {
                                Console.WriteLine($"{_clntEp} : StartGame");
                                if (_players.TryGetValue(datas[0], out var player))
                                {
                                    player.IsPlaying = true;
                                }

                                networkData.Data = Encoding.UTF8.GetBytes("Start");
                            }
                            else if (command == "Hit")
                            {
                                Console.WriteLine($"{_clntEp} : HIT");
                                networkData.Data = Encoding.UTF8.GetBytes("Slow");
                            }
                            else if (command == "Boost")
                            {
                                Console.WriteLine($"{_clntEp} : BOOST");
                                networkData.Data = Encoding.UTF8.GetBytes("SpeedUp");
                            }
                            else if (command == "HitE" || command == "BoostE")
                            {
                                Console.WriteLine($"{_clntEp} : State reset");
                                networkData.Data = Encoding.UTF8.GetBytes("ResetS");
                            }
                            else if (command == "Goal")
                            {
                                var score = float.Parse(datas[2]);
                                var scoreStr = $"{score}:F3";

                                Console.WriteLine($"{_clntEp} : EndGame (score : {scoreStr})");
                                if (_players.TryGetValue(datas[0], out var player))
                                {
                                    player.IsPlaying = false;
                                    if (!_dbManager.Goal(player.Uid, score))
                                    {
                                        Console.WriteLine($"{player.Uid} db failed");
                                    }
                                }
                            }
                            else // Distance
                            {
                                foreach (var p in _players)
                                {
                                    if (p.Key == datas[0] && p.Value.IsPlaying)
                                    {
                                        p.Value.Distance = datas[2].ToString();
                                        p.Value.Time = datas[3].ToString();
                                    }
                                }
                            }
                        }
                        break;
                    case Header.DBLogin:
                        {
                            var uid = datas[0];
                            var upw = datas[1];
                            if (_dbManager.Login(uid, upw))
                            {
                                networkData.Data = Encoding.UTF8.GetBytes("Success");
                                Player newPlayer = new()
                                {
                                    Distance = "100",
                                    Uid = uid,
                                    Time = "0",
                                    IsPlaying = false
                                };

                                _players.TryAdd(uid, newPlayer);
                            }
                            else
                            {
                                networkData.Data = Encoding.UTF8.GetBytes("Fail");
                            }
                        }

                        break;
                    case Header.DBSignUp:
                        {
                            var uid = datas[0];
                            var upw = datas[1];
                            if (_dbManager.SignUp(uid, upw))
                            {
                                networkData.Data = Encoding.UTF8.GetBytes("Success");
                                Console.WriteLine($"{_clntEp} : SignUp Requerst Success");
                            }
                            else
                            {
                                networkData.Data = Encoding.UTF8.GetBytes("Fail");
                                Console.WriteLine($"{_clntEp} : SignUp Requerst Fail");
                            }
                        }

                        break;

                    case Header.Quit:
                        string playerUid = datas[0];
                        Console.WriteLine($"{_clntEp} : quit (uid - {datas[0]})");
                        _players.TryRemove(playerUid, out _);
                        break;

                    case Header.Update:
                        string userInfo = "";
                        var availablePlayerList = _players.Values.Where(x => x.IsPlaying).ToList();
                        foreach (var p in availablePlayerList)
                        {
                            string uidInPlayers = p.Uid;
                            string distanceInPlayers = p.Distance;
                            string timeInPlayers = p.Time;
                            userInfo += $"{uidInPlayers} {distanceInPlayers:F3} {timeInPlayers:F3},";
                        }

                        networkData.Data = Encoding.UTF8.GetBytes(userInfo);
                        break;

                    case Header.Best:
                        {
                            var uid = datas[0];
                            var result = _dbManager.Best(uid);
                            networkData.Data = result is not null ? result : Encoding.UTF8.GetBytes("");
                        }

                        break;

                    default:
                        Console.WriteLine($"{_clntEp} : Header Error");
                        break;
                }

                SendMessage(networkData);
                CheckServerState();
            }
        }

        private void SendMessage(NetworkData networkData)
        {
            byte[] sendData = new byte[1024];
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(NetworkData));
            StringWriter stringWriter = new StringWriter();

            xmlSerializer.Serialize(stringWriter, networkData);
            string xmlString = stringWriter.ToString();

            sendData = Encoding.UTF8.GetBytes(xmlString);
            _servSock.SendTo(sendData, _clntEp);
        }

        private void CheckServerState()
        {
            // 키 입력을 받았을 때 쓰레드가 종료되기 위해 확인
            if (Console.KeyAvailable)
            {
                ConsoleKeyInfo keyInfo = Console.ReadKey(true);
                if (keyInfo.Key == ConsoleKey.Escape)
                {
                    SetFlag(false);
                }
            }
        }
    }
}
