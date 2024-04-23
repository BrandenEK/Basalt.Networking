using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace Basalt.Networking;

public class Client
{
    private readonly TcpClient _client;
    private readonly Thread _thread;

    private bool _shouldStop = false;

    public string Ip { get; }
    public int Port { get; }

    public Client(string ip, int port)
    {
        _client = new TcpClient(ip, port);
        _thread = StartReadThread();

        Ip = ip;
        Port = port;
    }

    public void Disconnect()
    {
        _client.Close();
        _shouldStop = true;
    }

    public void Send(byte[] data)
    {
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
        while (!_shouldStop)
        {
            try
            {
                Logger.Info("Client: Beginning read step");
                ReadStep();
            }
            catch { }

            Thread.Sleep(READ_INTERVAL);
        }
    }

    private void ReadStep()
    {
        Logger.Warn($"Client connection: {_client.Client.IsConnected()}");
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

    private const int READ_INTERVAL = 1000;
}
