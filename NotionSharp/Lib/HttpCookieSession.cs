using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Flurl.Http;
using Polly;
using Polly.Retry;

namespace NotionSharp
{
    public class HttpCookieSession
    {
        private readonly FlurlClient flurlClient;
        
        public CookieJar Cookies { get; } = new CookieJar();

        public HttpCookieSession(Action<FlurlClient> configure)
        {
            flurlClient = new FlurlClient(new HttpClient(new PolicyHandler()));
            configure(flurlClient);
        }

        public IFlurlRequest CreateRequest(Uri uri)
            => new FlurlRequest(uri) { Client = flurlClient }.WithCookies(Cookies);

        public IFlurlRequest CreateRequest(string uri)
            => new FlurlRequest(uri) { Client = flurlClient }.WithCookies(Cookies);

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
}