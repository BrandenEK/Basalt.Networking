using System;
using System.Text;
using System.Threading;

namespace Basalt.Networking;

internal class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("Hello, World!");

        var server = new Server(8989);

        var client = new Client("192.168.1.117", 8989);

        Thread.Sleep(2000);

        client.Send(Encoding.UTF8.GetBytes("Sending to server"));

        Thread.Sleep(2000);

        server.Broadcast(Encoding.UTF8.GetBytes("Sending to client"));

        Console.WriteLine("Press any key to terminate...");
        Console.ReadKey();
    }
}
