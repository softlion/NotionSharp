using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Flurl.Http;
using Polly;
using Polly.Retry;

namespace NotionSharp.Lib
{
    public class HttpSession
    {
        private readonly FlurlClient flurlClient;

        public HttpSession(Action<FlurlClient> configure)
        {
            flurlClient = new FlurlClient(new HttpClient(new PolicyHandler()));
            configure(flurlClient);
        }

        public FlurlRequest CreateRequest(Uri uri)
            => new FlurlRequest(uri) { Client = flurlClient };

        public FlurlRequest CreateRequest(string uri)
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