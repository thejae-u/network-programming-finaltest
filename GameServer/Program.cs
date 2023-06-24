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
    class Program
    {
        private static Socket servSock;
        private static IPEndPoint servEp;
        private static EndPoint clntEp;

        private static bool isRunning = true;

        public enum Header
        {
            PlayerInput,
            GameData,
            ETC
        }

        private class NetworkData
        {
            public Header head { get; set; }
            public byte[] data { get; set; }
        }

        static void Main(string[] args)
        {
            Thread serverThread = new Thread(StartServer);
            serverThread.Start();
            while (isRunning && !Console.KeyAvailable)
            {
                Thread.Sleep(100);
            }
            isRunning = false;
            serverThread.Join();
            Console.WriteLine("Server Stopped");

            Environment.Exit(1);
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
            byte[] rcvData = new byte[1024];
            while (isRunning)
            {
                int nRcvd = servSock.ReceiveFrom(rcvData, ref clntEp);

                NetworkData networkData;
                using (MemoryStream stream = new MemoryStream(rcvData, 0, nRcvd))
                {
                    BinaryFormatter formatter = new BinaryFormatter();
                    networkData = (NetworkData)formatter.Deserialize(stream);
                }

                Header rcvHeader = networkData.head;
                string rcvString = Encoding.UTF8.GetString(networkData.data);

                Console.WriteLine($"Rcvd Data from {clntEp} : {rcvHeader} {rcvString}");

                if (Console.KeyAvailable)
                {
                    ConsoleKeyInfo keyInfo = Console.ReadKey(true);
                    if (keyInfo.Key == ConsoleKey.Escape)
                    {
                        isRunning = false;
                    }
                }
            }
        }
    }
}
