using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
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
        static void Main(string[] args)
        {
            Thread serverThread = new Thread(StartServer);
            serverThread.Start();
            Console.ReadLine();
            serverThread.Abort();
            Environment.Exit(1);
        }

        private static void StartServer(object obj)
        {
            Socket servSock = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            IPEndPoint servEp = new IPEndPoint(IPAddress.Any, 55000);

            servSock.Bind(servEp);

            EndPoint clntEp = new IPEndPoint(IPAddress.None, 0);
            byte[] rcvData = new byte[1024];
            Console.WriteLine("Server Started...");

            while (true)
            {
                int nRcv = servSock.ReceiveFrom(rcvData, ref clntEp);
                string rcvDataStr = Encoding.UTF8.GetString(rcvData, 0, nRcv);

                Console.WriteLine($"Data From {clntEp} : {rcvDataStr}");

                servSock.SendTo(rcvData, clntEp);
            }
        }
    }
}
