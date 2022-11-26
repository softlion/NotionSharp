using System;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using FluentRest.Http;
using FluentRest.Http.Configuration;
using Polly;
using Polly.Retry;

namespace NotionSharp.ApiClient.Lib;

internal class JsonLowerCaseNamingPolicy : JsonNamingPolicy
{
    public override string ConvertName(string name) => name.ToLowerInvariant();
}
    
public class HttpNotionSession
{
    private readonly FluentRestClient flurlClient;

    public static JsonSerializerOptions NotionJsonSerializationOptions { get; } = new (JsonSerializerDefaults.Web)
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = new JsonLowerCaseNamingPolicy()
    };
        
    public HttpNotionSession(Action<FluentRestClient> configure)
    {
        flurlClient = new (new HttpClient(new PolicyHandler()))
        {
            Settings = new () { 
                JsonSerializer = new SystemTextJsonSerializer(NotionJsonSerializationOptions),
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
        => new FluentRestRequest(uri) { Client = flurlClient };

    public IFluentRestRequest CreateRequest(string uri)
        => new FluentRestRequest(uri) { Client = flurlClient };

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
}