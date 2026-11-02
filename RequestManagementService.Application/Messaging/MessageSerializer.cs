using System.Diagnostics.CodeAnalysis;
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

    public bool TryDeserialize<T>(ReadOnlyMemory<byte> body, [NotNullWhen(true)] out T? message)
        where T : class
    {
        try
        {
            message = JsonSerializer.Deserialize<T>(body.Span, Options);
        }
        catch (JsonException)
        {
            message = null;
        }

        return message is not null;
    }
}
