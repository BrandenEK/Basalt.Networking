using System;

namespace Basalt.Networking.Serializers;

public class Int8Serializer : ITypeSerializer
{
    public object? Deserialize(byte[] bytes)
    {
        return bytes[0];
    }

    public byte[] Serialize(object? value)
    {
        return [(byte)(value ?? 0)];
    }
}

public class Int64Serializer : ITypeSerializer
{
    public object? Deserialize(byte[] bytes)
    {
        return BitConverter.ToInt64(bytes);
    }

    public byte[] Serialize(object? value)
    {
        return BitConverter.GetBytes((long)(value ?? 0));
    }
}
