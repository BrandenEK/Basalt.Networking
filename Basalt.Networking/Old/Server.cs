
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

namespace Basalt.Networking.Old;

public class Server
{
    private Dictionary<string, Listener> _listeners = new();

    // Event handlers

    public void Start(int port)
    {
        IPAddress ip = IPAddress.Any;

        Listener listener = new Listener(this, ip, port);
        _listeners.Add(ip.ToString(), listener);
    }

    public void Stop()
    {
        foreach (var listener in _listeners.Values)
        {
            listener.Stop();
        }

        _listeners.Clear();
    }

    public void Send(string ip, byte[] data)
    {
        if (_listeners.TryGetValue(ip, out Listener listener))
        {
            listener.Client.GetStream().Write(data, 0, data.Length);
        }
    }
}
