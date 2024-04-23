using System;
using Basalt.Networking.Old;

namespace Basalt.Networking;

internal class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("Hello, World!");

#if DEBUG
        Client c = new Client();
        c.Connect("localhost", 8989);
#else
        Listener l = new(null, System.Net.IPAddress.Any, 8989);
#endif
        Console.WriteLine("Press any key to terminate...");
        Console.ReadKey();
    }
}
