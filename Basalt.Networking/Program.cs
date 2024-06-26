﻿using System;
using System.Threading;
using Basalt.Networking.Client;
using Basalt.Networking.Serializers;
using Basalt.Networking.Server;

namespace Basalt.Networking;

internal class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("Basalt Networking Interface");

        var serializer = new PacketSerializer();
        var packet = new PingPacket()
        {
            Ticks = DateTime.Now.Ticks
        };

        Logger.Error(serializer.Serialize(packet).Length);

#if DEBUG
        //RunClient();
#else
        RunServer();
#endif

        Console.WriteLine("Press any key to terminate...");
        Console.ReadKey();
    }

    static void RunClient()
    {
        var client = new NetworkClient("192.168.1.117", 8989);

        while (client.IsActive)
        {
            client.Send(BitConverter.GetBytes(DateTime.Now.Ticks));
            Thread.Sleep(500);
        }
    }

    static void RunServer()
    {
        double totalPing = 0;
        int totalAmount = 0;

        var server = new NetworkServer(8989);

        while (server.IsActive)
        {
            var bytes = server.Receive();

            if (bytes.Length != 0)
            {
                long sendTime = BitConverter.ToInt64(bytes, 0);
                TimeSpan span = new(DateTime.Now.Ticks - sendTime);

                totalPing += span.TotalMilliseconds;
                totalAmount++;

                Logger.Error($"Current ping: {span.TotalMilliseconds} ms");
                Logger.Warn($"Average ping: {totalPing / totalAmount}");
            }

            Thread.Sleep(16);
        }
    }
}

public class PingPacket : IPacket
{
    public byte Id => 0;

    public long Ticks { get; set; }
}
