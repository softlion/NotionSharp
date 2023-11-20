using System;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using FluentRest.Http;
using FluentRest.Http.Configuration;
using FluentRest.Rest.Configuration;
using Polly;
using Polly.Retry;

namespace NotionSharp.ApiClient.Lib;

internal class JsonLowerCaseNamingPolicy : JsonNamingPolicy
{
    public override string ConvertName(string name) => name.ToLowerInvariant();
}

[JsonSourceGenerationOptions(
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    PropertyNameCaseInsensitive = true,
    PropertyNamingPolicy = JsonKnownNamingPolicy.SnakeCaseLower,
    GenerationMode = JsonSourceGenerationMode.Default
    )]
[JsonSerializable(typeof(Parent))]
[JsonSerializable(typeof(RichText))]
[JsonSerializable(typeof(Mention))]
[JsonSerializable(typeof(PageRef))]
[JsonSerializable(typeof(DatabaseRef))]
[JsonSerializable(typeof(DateProp))]
[JsonSerializable(typeof(RichTextAnnotation))]
[JsonSerializable(typeof(RichTextText))]
[JsonSerializable(typeof(RichTextLink))]
[JsonSerializable(typeof(RichTextEquation))]
#region Blocks
[JsonSerializable(typeof(Block))]
[JsonSerializable(typeof(BlockText))]
[JsonSerializable(typeof(BlockTextAndChildren))]
[JsonSerializable(typeof(BlockTextAndChildrenAndCheck))]
[JsonSerializable(typeof(BlockChildPage))]
[JsonSerializable(typeof(BlockImage))]
[JsonSerializable(typeof(External))]
#endregion
[JsonSerializable(typeof(Bot))]
[JsonSerializable(typeof(Page))]
[JsonSerializable(typeof(Page.PageParentWorkspace))]
[JsonSerializable(typeof(Page.PageParentPage))]
[JsonSerializable(typeof(Page.PageParentDatabase))]
[JsonSerializable(typeof(PropertyItem))]
[JsonSerializable(typeof(Person))]
[JsonSerializable(typeof(PropertyTitle))]
[JsonSerializable(typeof(User))]
#region Requests
[JsonSerializable(typeof(FilterOptions))]
[JsonSerializable(typeof(PagingOptions))]
[JsonSerializable(typeof(SearchRequest))]
[JsonSerializable(typeof(SortOptions))]
#endregion
[JsonSerializable(typeof(PaginationResult<Page>))]
[JsonSerializable(typeof(PaginationResult<User>))]
[JsonSerializable(typeof(PaginationResult<Block>))]
[JsonSerializable(typeof(PaginationResult<PropertyItem>))]
internal partial class NotionJsonContext : JsonSerializerContext { }



/// <summary>
/// Manage the connection to the Notion API
/// </summary>
/// <remarks>
/// https://www.postman.com/notionhq/workspace/notion-s-api-workspace/collection/15568543-d990f9b7-98d3-47d3-9131-4866ab9c6df2
/// https://developers.notion.com/reference/status-codes#error-codes
/// </remarks>
public class HttpNotionSession
{
    private readonly FluentRestClient flurlClient;
    public ISerializer JsonSerializer => flurlClient.Settings.JsonSerializer;

    public static JsonSerializerOptions NotionJsonSerializationOptions { get; } = new (JsonSerializerDefaults.General)
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = new JsonLowerCaseNamingPolicy(),
        TypeInfoResolver = NotionJsonContext.Default
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