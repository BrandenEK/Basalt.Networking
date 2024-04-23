using System;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace Basalt.Networking.Old;

public class Client
{
    private Thread? _thread;
    private TcpClient? _client;

    private bool _running = true;

    public event EventHandler<Packet> OnReceive;

    public bool Connect(string ip, int port)
    {
        // Validate ip is real

        Console.WriteLine($"Attempting to connect to {ip}:{port}");

        try
        {
            _client = new TcpClient();
            _client.Connect(ip, port);

            Console.WriteLine($"Successfully connected to {ip}:{port}");
            StartReadThread();
            return true;
        }
        catch (SocketException e)
        {
            Console.WriteLine($"Failed to connect to {ip}:{port} ({e.Message})");
            return false;
        }
    }

    public void Disconnect()
    {
        if (_client != null)
        {
            _client.Close();
            _client = null;
        }

        _running = false;
    }

    public void Send(byte[] data)
    {
        if (_client == null)
            return;

        _client.GetStream().Write(data, 0, data.Length);
    }

    private void StartReadThread()
    {
        if (_thread != null)
            return;

        _thread = new Thread(ReadLoop);
        _thread.IsBackground = true;
        _thread.Start();
    }

    private void ReadLoop()
    {
        while (_running)
        {
            try
            {
                ReadStep();
            }
            catch { }

            Thread.Sleep(READ_INTERVAL);
        }

        _thread = null;
    }

    private void ReadStep()
    {
        Console.WriteLine("Read step");

        if (_client == null || !_client.Connected)
            return;

        Console.WriteLine("Client is connected");

        Send(Encoding.UTF8.GetBytes("Hello"));

        if (_client.Available == 0)
            return;

        Console.WriteLine("Client has data to receive");

        byte[] buffer = new byte[_client.Available];
        _client.Client.Receive(buffer, 0, buffer.Length, SocketFlags.None);

        Console.WriteLine($"Client: {Encoding.UTF8.GetString(buffer)}");

        //OnReceive(this, new Packet(buffer));
    }

    private const int READ_INTERVAL = 1600;
}

public class Packet(byte[] data) : EventArgs
{
    public byte[] Data { get; } = data;
}