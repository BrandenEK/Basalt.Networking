using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace Basalt.Networking.Old;

internal class Listener
{
    private Server _server;
    private TcpListener _listener;

    private List<TcpClient> _clients = new();

    private Thread? _thread;

    private bool _running = true;

    public TcpClient Client { get; private set; }

    public Listener(Server server, IPAddress ip, int port)
    {
        _server = server;
        _listener = new TcpListener(ip, port);
        _listener.Start();

        StartReadThread();
    }

    public void Stop()
    {
        _running = false;
    }

    public void Send(string ip, byte[] data)
    {
        foreach (var client in _clients)
        {
            client.GetStream().Write(data, 0, data.Length);
        }

        //if (_listeners.TryGetValue(ip, out Listener listener))
        //{
        //    listener.Client.GetStream().Write(data, 0, data.Length);
        //}
    }

    private void StartReadThread()
    {
        if (_thread != null)
            return;

        _thread = new Thread(ReadLoop);
        _thread.IsBackground = true;
        _thread.Start();
    }

    private void ReadLoop(object state)
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

        if (_listener == null)
            return;

        if (_listener.Pending())
        {
            var client = _listener.AcceptTcpClient();
            _clients.Add(client);
            Console.WriteLine($"Accepting new client: {client.Client.RemoteEndPoint}");
        }


        //if (_listener.Server.Available == 0)
        //{
        //    Thread.Sleep(READ_INTERVAL);
        //    return;
        //}


        //byte[] buffer = new byte[_listener.Server.Available];
        //_listener.Server.Receive(buffer, 0, buffer.Length, SocketFlags.None);
        List<TcpClient> toRemove = new();

        foreach (var client in _clients)
        {
            if (!client.Connected)
            {
                toRemove.Add(client);
                continue;
            }

            Console.WriteLine("Listener is connected");

            //Send("", Encoding.UTF8.GetBytes("World"));

            if (client.Available == 0)
            {
                //Thread.Sleep(READ_INTERVAL);
                continue;
            }

            Console.WriteLine("Listener has data to receive");

            byte[] buffer = new byte[client.Available];
            client.Client.Receive(buffer, 0, buffer.Length, SocketFlags.None);

            Console.WriteLine($"Listener: {Encoding.UTF8.GetString(buffer)}");
        }

        foreach (var client in toRemove)
        {
            Console.WriteLine("Disconnected client");
            _clients.Remove(client);
        }

        //OnReceive(this, new Packet(buffer));

    }

    private const int READ_INTERVAL = 1600;
}
