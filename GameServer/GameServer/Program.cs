using System;

namespace GameServer
{
    class Program
    {
        static void Main(string[] args)
        {
            Server server = new Server(55000);
            server.StartServer();

            // 키 입력을 받으면 서버를 종료
            while (server.GetFlag() && !Console.KeyAvailable)
            {
                Task.Delay(100);
            }

            server.Stop();
            Console.WriteLine("Server Stopped");
            Environment.Exit(1);
        }
    }
}
