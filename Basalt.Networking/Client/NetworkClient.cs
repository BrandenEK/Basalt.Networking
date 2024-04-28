using System.Net.Sockets;

namespace Basalt.Networking.Client;

public class NetworkClient
{
    private readonly TcpClient _client;
    public bool IsActive { get; private set; }

    public string Ip { get; }
    public int Port { get; }

    public NetworkClient(string ip, int port)
    {
        _client = new TcpClient(ip, port);
        _client.NoDelay = NetworkProperties.NoDelay;
        _client.Client.NoDelay = NetworkProperties.NoDelay;

        Ip = ip;
        Port = port;

        IsActive = true;
    }

    public void Disconnect()
    {
        _client.Close();

        IsActive = false;
    }

    public void Send(byte[] data)
    {
        if (!IsActive)
            throw new NetworkSendException();

        CheckConnectionStatus();

        _client.GetStream().Write(data, 0, data.Length);
    }

    public byte[] Receive()
    {
        if (!IsActive)
            throw new NetworkReceiveException();

        CheckConnectionStatus();

        if (_client.Available == 0)
            return [];

        byte[] buffer = new byte[_client.Available];
        _client.Client.Receive(buffer, 0, buffer.Length, SocketFlags.None);
        return buffer;
    }

    private void CheckConnectionStatus()
    {
        if (IsActive && !_client.Client.IsConnected())
        {
            Disconnect();
            throw new NetworkDisconnectException();
        }
    }
}
