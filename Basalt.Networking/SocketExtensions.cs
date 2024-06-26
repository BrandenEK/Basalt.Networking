﻿using System.Net.Sockets;

namespace Basalt.Networking;

internal static class SocketExtensions
{
    public static bool IsConnected(this Socket s)
    {
        return !((s.Poll(1000, SelectMode.SelectRead) && (s.Available == 0)) || !s.Connected);
    }
}
