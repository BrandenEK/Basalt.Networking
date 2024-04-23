using System;

namespace Basalt.Networking;

public class TcpNetworkException(string message) : Exception(message) { }

public class TcpClientException(string message) : TcpNetworkException(message) { }

public class TcpServerException(string message) : TcpNetworkException(message) { }
