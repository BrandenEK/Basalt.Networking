using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace Basalt.Networking.Client;

public class NetworkClient
{
    private readonly TcpClient _client;
    private readonly Thread _thread;
    private bool _active = false;

    public string Ip { get; }
    public int Port { get; }

    public NetworkClient(string ip, int port)
    {
        _client = new TcpClient(ip, port);

        Ip = ip;
        Port = port;

        _thread = StartReadThread();
        _active = true;
    }

    public void Disconnect()
    {
        _client.Close();

        _active = false;
    }

    public void Send(byte[] data)
    {
        if (!_active)
            throw new TcpClientException("Can not send data on inactive client");

        _client.GetStream().Write(data, 0, data.Length);
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
                Logger.Info("Client: Beginning read step");
                ReadStep();
            }
            catch { }

            Thread.Sleep(NetworkProperties.ReadIntervalMilliseconds);
        }
    }

    private void ReadStep()
    {
        if (!_client.Client.IsConnected())
        {
            Logger.Error("Disconnected from server");
            Disconnect();
            return;
        }

        if (_client.Available == 0)
            return;

        byte[] buffer = new byte[_client.Available];
        _client.Client.Receive(buffer, 0, buffer.Length, SocketFlags.None);

        Logger.Error($"Received: {Encoding.UTF8.GetString(buffer)}");
    }
}
