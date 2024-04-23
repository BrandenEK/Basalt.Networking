using System;

namespace Basalt.Networking;

internal class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("Hello, World!");

        var server = new Server(8989);

        Console.WriteLine("Press any key to terminate...");
        Console.ReadKey();
    }
}
