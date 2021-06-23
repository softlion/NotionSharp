using System;
using System.Buffers;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Flurl.Http;

namespace NotionSharp.ApiClient.Lib
{
    public static class JsonExtensions
    {
        public static T ToObject<T>(this JsonElement element, JsonSerializerOptions? options = null)
            => (T)element.ToObject(typeof(T), options);
        
        public static object? ToObject(this JsonElement element, Type targetType, JsonSerializerOptions? options = null)
        {
            var bufferWriter = new ArrayBufferWriter<byte>();
            using var writer = new Utf8JsonWriter(bufferWriter);
            element.WriteTo(writer);
            writer.Flush();

            return JsonSerializer.Deserialize(bufferWriter.WrittenSpan, targetType, options);
        }
        
        public static async Task<T> GetJson<T>(this IFlurlRequest request, CancellationToken cancel = default) where T: BaseObject, new()
        {
            var response = await request.SendAsync(HttpMethod.Get, cancellationToken: cancel, completionOption: HttpCompletionOption.ResponseContentRead).ConfigureAwait(false);
            var json = await response.GetStringAsync().ConfigureAwait(false);
            var o = request.Settings.JsonSerializer.Deserialize<T>(json) ?? new T();
            o.JsonOriginal = json;
            return o;
        }
    }
}