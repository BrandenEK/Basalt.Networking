using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Basalt.Networking.Serializers;

public class PacketSerializer
{
    private readonly Dictionary<Type, Func<object?, byte[]>> _serializers = new();

    public PacketSerializer()
    {
        AddSerializer(typeof(byte), obj => new byte[] { (byte)(obj ?? 0) });
        AddSerializer(typeof(int), obj => BitConverter.GetBytes((int)(obj ?? 0)));
        AddSerializer(typeof(long), obj => BitConverter.GetBytes((long)(obj ?? 0)));
    }

    public void AddSerializer(Type type, Func<object?, byte[]> serializer)
    {
        if (!_serializers.ContainsKey(type))
            _serializers.Add(type, serializer);
    }

    public byte[] Serialize(IPacket packet)
    {
        IEnumerable<byte> packetBytes = SerializePacket(packet);
        ushort length = (ushort)packetBytes.Count();

        return new byte[] { packet.Id }
            .Concat(BitConverter.GetBytes(length))
            .Concat(packetBytes)
            .ToArray();
    }

    private IEnumerable<byte> SerializePacket(IPacket packet)
    {
        return packet.GetType()
            .GetProperties(BindingFlags.Instance | BindingFlags.Public)
            .Where(p => p.Name != "Id")
            .SelectMany(p => SerializeProperty(p.PropertyType, p.GetValue(packet)));
    }

    private byte[] SerializeProperty(Type type, object? value)
    {
        return _serializers.TryGetValue(type, out var serializer)
            ? serializer(value)
            : throw new Exception($"Failed to serialize unsupported type: {type.FullName}");
    }
}
