using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using System.Threading;
using System.Threading.Tasks;
using FluentRest.Http;
using FluentRest.Http.Configuration;
using FluentRest.Rest.Configuration;
using Polly;
using Polly.Retry;

namespace NotionSharp.ApiClient.Lib;



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
[JsonSerializable(typeof(NotionFile))]
[JsonSerializable(typeof(NotionFileContent))]
[JsonSerializable(typeof(NotionFileExternal))]
[JsonSerializable(typeof(NotionEmoji))]
#endregion
[JsonSerializable(typeof(Bot))]

[JsonSerializable(typeof(Page))]
[JsonSerializable(typeof(PageParent))]
[JsonSerializable(typeof(PageParentWorkspace))]
[JsonSerializable(typeof(PageParentPage))]
[JsonSerializable(typeof(PageParentDatabase))]
[JsonSerializable(typeof(TitlePropertyItem))]
[JsonSerializable(typeof(RichTextPropertyItem))]
[JsonSerializable(typeof(NumberPropertyItem))]

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
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        DictionaryKeyPolicy = JsonNamingPolicy.SnakeCaseLower,
        TypeInfoResolver = NotionJsonContext.Default.WithAddedModifier(ModifierIgnoreBlockChildren) 
        //JsonTypeInfoResolver.Combine(NotionJsonContext.Default) //.WithAddedModifier(AddNestedDerivedTypes))
    };

    public static JsonSerializerOptions NotionJsonFullSerializationOptions { get; } = new (JsonSerializerDefaults.General)
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        DictionaryKeyPolicy = JsonNamingPolicy.SnakeCaseLower,
        TypeInfoResolver = NotionJsonContext.Default 
    };

    private static void ModifierIgnoreBlockChildren(JsonTypeInfo jsonTypeInfo)
    {
        // if (jsonTypeInfo.Type == typeof(PropertyItem))
        // {
        //     //JsonDerivedType ignores JsonConverter on target type
        //     var c = jsonTypeInfo.Converter;
        //     var i = 0;
        // }

        if (jsonTypeInfo.Type.IsAssignableTo(typeof(Block)))
        {
            var prop = jsonTypeInfo.Properties.FirstOrDefault(p => p.Name == nameof(Block.Children));
            if (prop is not null)
                prop.ShouldSerialize = (_, _) => false;
        }
    }

    // // Ne fait qu'ajouter JsonDerivedType au type de base. Ne peux pas changer le TypeDiscriminatorPropertyName.
    // static void AddNestedDerivedTypes(JsonTypeInfo jsonTypeInfo)
    // {
        // if (jsonTypeInfo.PolymorphismOptions is null) 
        //     return;
        // var derivedTypes = ( 
        //     from firstLevelType in jsonTypeInfo.PolymorphismOptions.DerivedTypes
        //     let secondLevelType = firstLevelType.DerivedType
        //     where Attribute.IsDefined(secondLevelType, typeof(JsonDerivedTypeAttribute))
        //     let jsonTypeInfo2 = secondLevelType.GetCustomAttributes<JsonPolymorphicAttribute>().FirstOrDefault()
        //     where jsonTypeInfo2 != null
        //     let discriminator = jsonTypeInfo2.TypeDiscriminatorPropertyName
        //     let attributes = secondLevelType.GetCustomAttributes<JsonDerivedTypeAttribute>()
        //     select secondLevelType)
        //     .ToList();
        //
        // var hashset = new HashSet<Type>(derivedTypes);
        // var queue = new Queue<Type>(derivedTypes);
        // while (queue.TryDequeue(out var derived))
        // {
        //     // Todo: handle discriminators
        //     jsonTypeInfo.PolymorphismOptions.DerivedTypes.Add(new (derived, derived.FullName));
        //     hashset.Add(derived); // FIXED: Add derived to the hashset in case it appears more than once.
        //
        //     var attribute = derived.GetCustomAttributes<JsonDerivedTypeAttribute>();
        //     foreach (var jsonDerivedTypeAttribute in attribute) 
        //         queue.Enqueue(jsonDerivedTypeAttribute.DerivedType);
        // }
    // }
        
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