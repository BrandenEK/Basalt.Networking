
namespace Basalt.Networking.Serializers;

public interface ITypeSerializer
{
    public byte[] Serialize(object? value);

    public object? Deserialize(byte[] bytes);
}
