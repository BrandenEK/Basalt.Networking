using System;
using System.Text;
using System.Threading;

namespace Basalt.Networking;

internal class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("Hello, World!");

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
        var client = new Client("192.168.1.117", 8989);

        Thread.Sleep(1000);

        client.Send(Encoding.UTF8.GetBytes("Sending to server"));

        Thread.Sleep(1000);

        client.Send(new byte[] { 5, 5 });
    }

    static void RunServer()
    {
        var server = new Server(8989);

        Thread.Sleep(1000);

        server.Broadcast(Encoding.UTF8.GetBytes("Sending to client"));

        Thread.Sleep(1000);
        Console.ReadKey();
        
        server.Disconnect();

        Thread.Sleep(1000);

        server.Broadcast(new byte[] {5 , 5});
    }
}
