using System;

namespace Basalt.Networking;

public class NetworkException(string message) : Exception(message) { }

public class NetworkSendException : NetworkException
{
    public NetworkSendException() : base("Failed to send data on inactive connection") { }
}

public class NetworkReceiveException : NetworkException
{
    public NetworkReceiveException() : base("Failed to receive data on inactive connection") { }
}

public class NetworkDisconnectException : NetworkException
{
    public NetworkDisconnectException() : base("Disconnected from remote connection") { }
}