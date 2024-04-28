using System.Collections.Generic;
using System.Net.Sockets;
using System.Net;
using System.Threading;
using System;
using System.Linq;

namespace Basalt.Networking.Server;

public class NetworkServerWithEvents
{
    private readonly TcpListener _listener;
    private readonly Thread _thread;
    private bool _active = false;

    private readonly Dictionary<string, TcpClient> _clients = new();

    public string Ip { get; }
    public int Port { get; }

    public NetworkServerWithEvents(int port)
    {
        _listener = new TcpListener(IPAddress.Any, port);
        _listener.Server.NoDelay = NetworkProperties.NoDelay;
        _listener.Start();

        Ip = _listener.LocalEndpoint.ToString()!;
        Port = port;

        _thread = StartReadThread();
        _active = true;
    }

    public void Disconnect()
    {
        foreach (var client in _clients.Values)
            client.Close();
        _clients.Clear();
        _listener.Stop();

        _active = false;
    }

    public void Send(string ip, byte[] data)
    {
        if (!_active)
            throw new TcpServerException("Can not send data on inactive server");

        if (_clients.TryGetValue(ip, out TcpClient? client))
        {
            client.GetStream().Write(data, 0, data.Length);
        }
    }

    public void Broadcast(byte[] data)
    {
        if (!_active)
            throw new TcpServerException("Can not send data on inactive server");

        foreach (var client in _clients.Values)
        {
            client.GetStream().Write(data, 0, data.Length);
        }
    }

    private Thread StartReadThread()
    {
        Thread thread = new Thread(ReadLoop);
        thread.IsBackground = true;
        thread.Start();

        return thread;
    }

    private void ReadLoop()
    {
        while (_active)
        {
            try
            {
                //Logger.Info("Server: Beginning read step");
                ReadStep();
            }
            catch { }

            Thread.Sleep(NetworkProperties.ReadIntervalMilliseconds);
        }
    }

    private void ReadStep()
    {
        // Check for new connections
        if (_listener.Pending())
        {
            TcpClient client = _listener.AcceptTcpClient();
            client.NoDelay = NetworkProperties.NoDelay;
            client.Client.NoDelay = NetworkProperties.NoDelay;
            _clients.Add(client.Client.RemoteEndPoint!.ToString()!, client);
            Logger.Warn($"Accepting new client: {client.Client.RemoteEndPoint}");
        }

        // Remove all clients that have been disconnected
        foreach (string ip in _clients.Where(kvp => !kvp.Value.Client.IsConnected()).Select(kvp => kvp.Key))
        {
            Logger.Warn($"Client has been disconnected: {ip}");
            _clients.Remove(ip);
        }

        // Read data from all client streams
        foreach (var client in _clients.Values)
        {
            if (client.Available == 0)
                continue;

            byte[] buffer = new byte[client.Available];
            client.Client.Receive(buffer, 0, buffer.Length, SocketFlags.None);

            long sendTime = BitConverter.ToInt64(buffer, 0);
            TimeSpan span = new(DateTime.Now.Ticks - sendTime);

            totalPing += span.TotalMilliseconds;
            totalAmount++;

            Logger.Error($"Current ping: {span.TotalMilliseconds} ms");
            Logger.Warn($"Average ping: {totalPing / totalAmount}");
        }
    }

    private static double totalPing = 0;
    private static int totalAmount = 0;
}
