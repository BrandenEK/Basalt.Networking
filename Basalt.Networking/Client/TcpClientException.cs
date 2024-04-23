using System;

namespace Basalt.Networking.Client;

public class TcpClientException(string message) : Exception(message) { }
