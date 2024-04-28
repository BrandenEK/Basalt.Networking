using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;

namespace Basalt.Networking.Server;

public class NetworkServer
{
    private readonly TcpListener _listener;
    public bool IsActive { get; private set; }

    private readonly Dictionary<string, TcpClient> _clients = new();

    public string Ip { get; }
    public int Port { get; }

    public NetworkServer(int port)
    {
        _listener = new TcpListener(IPAddress.Any, port);
        _listener.Server.NoDelay = NetworkProperties.NoDelay;
        _listener.Start();

        Ip = _listener.LocalEndpoint.ToString()!;
        Port = port;

        IsActive = true;
    }

    public void Stop()
    {
        foreach (var client in _clients.Values)
            client.Close();
        _clients.Clear();
        _listener.Stop();

        IsActive = false;
    }

    public void Send(string ip, byte[] data)
    {
        if (!IsActive)
            throw new NetworkSendException();

        if (_clients.TryGetValue(ip, out TcpClient? client))
        {
            client.GetStream().Write(data, 0, data.Length);
        }
    }

    public void Broadcast(byte[] data)
    {
        if (!IsActive)
            throw new NetworkSendException();

        foreach (var client in _clients.Values)
        {
            client.GetStream().Write(data, 0, data.Length);
        }
    }

    public byte[] Receive()
    {
        if (!IsActive)
            throw new NetworkReceiveException();

        // Check for new connections
        if (_listener.Pending())
        {
            ConnectClient(_listener.AcceptTcpClient());
        }

        // Remove all clients that have been disconnected
        foreach (string ip in _clients.Where(kvp => !kvp.Value.Client.IsConnected()).Select(kvp => kvp.Key))
        {
            DisconnectClient(ip);
        }

        // Read data from all client streams
        foreach (var client in _clients.Values)
        {
            if (client.Available == 0)
                continue;

            byte[] buffer = new byte[client.Available];
            client.Client.Receive(buffer, 0, buffer.Length, SocketFlags.None);
            return buffer;
            // This is wrong, need to do something for multiple clients
        }

        return [];
    }

    private void ConnectClient(TcpClient client)
    {
        client.NoDelay = NetworkProperties.NoDelay;
        client.Client.NoDelay = NetworkProperties.NoDelay;
        _clients.Add(client.Client.RemoteEndPoint!.ToString()!, client);
        Logger.Warn($"Accepting new client: {client.Client.RemoteEndPoint}");
    }

    private void DisconnectClient(string ip)
    {
        Logger.Warn($"Client has been disconnected: {ip}");
        _clients.Remove(ip);
    }
}
