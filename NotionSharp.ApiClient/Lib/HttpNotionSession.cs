using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Flurl.Http;
using Flurl.Http.Configuration;
using Polly;
using Polly.Retry;

namespace NotionSharp.ApiClient.Lib
{
    /// <summary>
    /// Flurl depends on newtonsoft json. Use System.Text.Json instead.
    /// </summary>
    internal class TextJsonSerializer : ISerializer
    {
        private readonly JsonSerializerOptions? options;

        public TextJsonSerializer(JsonSerializerOptions? options = null) => this.options = options;
        public T Deserialize<T>(string s) => JsonSerializer.Deserialize<T>(s, options)!;
        public string Serialize(object obj) => JsonSerializer.Serialize(obj, options);

        public T Deserialize<T>(Stream stream)
        {
            using var reader = new StreamReader(stream);
            return Deserialize<T>(reader.ReadToEnd());
        }
    }
    
    internal class JsonLowerCaseNamingPolicy : JsonNamingPolicy
    {
        public override string ConvertName(string name) => name.ToLowerInvariant();
    }
    
    public class HttpNotionSession
    {
        private readonly FlurlClient flurlClient;
        
        public HttpNotionSession(Action<FlurlClient> configure)
        {
            flurlClient = new FlurlClient(new HttpClient(new PolicyHandler()))
            {
                Settings = new ClientFlurlHttpSettings { 
                    JsonSerializer = new TextJsonSerializer(new JsonSerializerOptions()
                    {
                        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault,
                        PropertyNameCaseInsensitive = true,
                        PropertyNamingPolicy = new JsonLowerCaseNamingPolicy()
                    }),
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

        public IFlurlRequest CreateRequest(Uri uri)
            => new FlurlRequest(uri) { Client = flurlClient };

        public IFlurlRequest CreateRequest(string uri)
            => new FlurlRequest(uri) { Client = flurlClient };

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
}