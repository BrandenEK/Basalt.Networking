using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace Basalt.Networking;

public class Server
{
    private readonly TcpListener _listener;
    private readonly Thread _thread;

    private readonly Dictionary<string, TcpClient> _clients = new();
    private bool _shouldStop = false;

    public string Ip { get; }
    public int Port { get; }

    public Server(int port)
    {
        _listener = new TcpListener(IPAddress.Any, port);
        _listener.Start();
        _thread = StartReadThread();

        Ip = _listener.LocalEndpoint.ToString()!;
        Port = port;
    }

    public void Disconnect()
    {
        foreach (var client in _clients.Values)
            client.Close();
        _clients.Clear();
        _listener.Stop();
        _shouldStop = true;
    }

    public void Send(string ip, byte[] data)
    {
        if (_clients.TryGetValue(ip, out TcpClient? client))
        {
            client.GetStream().Write(data, 0, data.Length);
        }
    }

    public void Broadcast(byte[] data)
    {
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
        while (!_shouldStop)
        {
            try
            {
                Logger.Info("Server: Beginning read step");
                ReadStep();
            }
            catch { }

            Thread.Sleep(READ_INTERVAL);
        }
    }

    private void ReadStep()
    {
        // Check for new connections
        if (_listener.Pending())
        {
            TcpClient client = _listener.AcceptTcpClient();
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

            Logger.Error($"Received: {Encoding.UTF8.GetString(buffer)}");
        }
    }

    private const int READ_INTERVAL = 1000;
}
