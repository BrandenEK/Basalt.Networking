using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Basalt.Networking.Serializers;

public class PacketSerializer
{
    private readonly Dictionary<Type, ITypeSerializer> _serializers = new();

    public PacketSerializer()
    {
        AddSerializer(typeof(byte), new Int8Serializer());
        AddSerializer(typeof(long), new Int64Serializer());
    }

    public void AddSerializer(Type type, ITypeSerializer serializer)
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
            ? serializer.Serialize(value)
            : throw new Exception($"Failed to serialize unsupported type: {type.FullName}");
    }
}
