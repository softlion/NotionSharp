using System.IO;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using FluentRest.Rest.Configuration;
using Polly;
using Polly.Retry;

namespace NotionSharp;

internal class SystemTextJsonSerializer : ISerializer
{
    private static readonly JsonSerializerOptions? DefaultOptions = new (JsonSerializerDefaults.General)
    {
        PropertyNameCaseInsensitive = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
    };
    private readonly JsonSerializerOptions? options;

    public SystemTextJsonSerializer(JsonSerializerOptions? options = null) => this.options = options ?? DefaultOptions;

    public string Serialize(object obj) => JsonSerializer.Serialize(obj, options);

    public T? Deserialize<T>(string s) => JsonSerializer.Deserialize<T>(s, options);

    public T? Deserialize<T>(Stream stream) => JsonSerializer.Deserialize<T>(stream, options);
}

public class HttpCookieSession
{
    private readonly FluentRestClient flurlClient;
        
    public CookieJar Cookies { get; } = new ();

    public HttpCookieSession(Action<FluentRestClient> configure)
    {
        flurlClient = new (new HttpClient(new PolicyHandler()))
        {
            Settings = new () { 
                JsonSerializer = new SystemTextJsonSerializer(),
#if DEBUG
                BeforeCall = call =>
                {
                    var request = call.Request;
                    var requestBody = call.RequestBody;
                    var i = 0;
                }, 
                OnError = call =>
                {
                    var exception = call.Exception;
                    var response = call.Response;
                    var i = 0;
                }, 
                AfterCall = call =>
                {
                    var exception = call.Exception;
                    var response = call.Response;
                    //var tt = response.ResponseMessage.Content.ReadAsStringAsync();
                    var i = 0;
                }  
#endif
            },
        };
        configure(flurlClient);
    }

    public IFluentRestRequest CreateRequest(Uri uri)
        => new FluentRestRequest(uri) { Client = flurlClient }.WithCookies(Cookies);

    public IFluentRestRequest CreateRequest(string uri)
        => new FluentRestRequest(uri) { Client = flurlClient }.WithCookies(Cookies);

    class PolicyHandler : DelegatingHandler
    {
        private readonly AsyncRetryPolicy<HttpResponseMessage> retryPolicy;

        public PolicyHandler()
        {
            InnerHandler = new HttpClientHandler();

            //retry on 502
            retryPolicy = Policy
                .HandleResult<HttpResponseMessage>(r => r.StatusCode == HttpStatusCode.BadGateway)
                .WaitAndRetryAsync(5, retry => TimeSpan.FromSeconds(0.3));
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            => retryPolicy.ExecuteAsync(ct => base.SendAsync(request, ct), cancellationToken);
    }

    public HttpCookieSession WithCookies(string originUrl, Dictionary<string, string> dictionary)
    {
        foreach (var kp in dictionary)
            Cookies.AddOrReplace(kp.Key, kp.Value, originUrl);

        return this;
    }
}