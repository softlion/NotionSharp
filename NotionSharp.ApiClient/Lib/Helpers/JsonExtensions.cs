using System;
using System.Buffers;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using FluentRest.Http;

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
        
        public static async Task<T> GetJson<T>(this IFluentRestRequest request, CancellationToken cancel = default) where T: NamedObject, new()
        {
            var response = await request.SendAsync(HttpMethod.Get, cancellationToken: cancel, completionOption: HttpCompletionOption.ResponseContentRead).ConfigureAwait(false);
            var json = await response.GetStringAsync().ConfigureAwait(false);

            try
            {
                var o = request.Settings.JsonSerializer.Deserialize<T>(json) ?? new T();
                o.JsonOriginal = json;
                return o;
            }
            catch(Exception e) when(e is not TimeoutException and not OperationCanceledException)
            {
                throw new NotionApiException(e, $"GetJson<{typeof(T).Name}>() failed. See InnerException for details.")
                {
                    Request = request,
                    Json = json,
                };
            }
        }
    }

    public class NotionApiException : Exception
    {
        public NotionApiException(Exception? innerException, string? message) : base(message, innerException) {}
        
        public IFluentRestRequest Request { get; init; } 
        public string Json { get; init; }
    }
}