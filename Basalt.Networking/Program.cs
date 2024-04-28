using System;
using System.Text;
using System.Threading;
using Basalt.Networking.Client;
using Basalt.Networking.Server;

namespace Basalt.Networking;

internal class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("Hello, World!");
        NetworkProperties.ReadIntervalMilliseconds = 1000;

#if DEBUG
        RunClient();
#else
        RunServer();
#endif

        Console.WriteLine("Press any key to terminate...");
        Console.ReadKey();
    }

    static void RunClient()
    {
        var client = new NetworkClient("192.168.1.117", 8989);

        while (true)
        {
            client.Send(BitConverter.GetBytes(DateTime.Now.Ticks));
            Thread.Sleep(500);
        }
    }

    static void RunServer()
    {
        var server = new NetworkServer(8989);
    }
}
