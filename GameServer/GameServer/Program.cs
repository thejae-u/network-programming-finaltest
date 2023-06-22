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

        static void StartServer()
        {
            Socket servSock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

        }
    }
}
