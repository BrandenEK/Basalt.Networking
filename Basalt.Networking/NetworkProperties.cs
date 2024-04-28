
namespace Basalt.Networking;

public static class NetworkProperties
{
    public static int ReadIntervalMilliseconds { get; set; } = 16;

    public static bool NoDelay { get; set; } = true;
}
