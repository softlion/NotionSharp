using System.Buffers;
using System.Text.Json;

namespace NotionSharp.ApiClient.Lib
{
    public static class JsonExtensions
    {
        public static T ToObject<T>(this JsonElement element, JsonSerializerOptions? options = null)
        {
            var bufferWriter = new ArrayBufferWriter<byte>();
            using var writer = new Utf8JsonWriter(bufferWriter);
            element.WriteTo(writer);

            return JsonSerializer.Deserialize<T>(bufferWriter.WrittenSpan, options);
        }
    }
}