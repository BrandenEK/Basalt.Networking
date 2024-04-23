using System;

namespace Basalt.Networking.Server;

public class TcpServerException(string message) : Exception(message) { }
