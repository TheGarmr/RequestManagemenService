using System.Text.Json;
using System.Text.Json.Serialization;

namespace RequestManagementService.Application.Messaging;

public sealed class MessageSerializer
{
    private static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() }
    };

    public byte[] Serialize<T>(T message)
    {
        return JsonSerializer.SerializeToUtf8Bytes(message, Options);
    }

    public T Deserialize<T>(ReadOnlyMemory<byte> body)
    {
        return JsonSerializer.Deserialize<T>(body.Span, Options)
            ?? throw new JsonException($"Message body cannot be deserialized to {typeof(T).Name}.");
    }
}
