using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace NotionSharp.ApiClient;

public static class PageExtensions
{
    /// <summary>
    /// If false, then the only property in Properties is "title"
    /// </summary>
    public static bool PageParentIsDatabase(this Page page) => page.Parent.Type == "database_id";

    public static TitlePropertyItem? Title(this Page page)
    {
        if (page.Properties?.TryGetValue("title", out var item) == true && item is TitlePropertyItem title)
            return title;
        return null;
    }
}

public class Page : NamedObject, IBlockId
{
    public Page()
    {
        Object = "page";
    }
    public string Id { get; init; }
            
    public DateTimeOffset CreatedTime { get; init; }
    public DateTimeOffset LastEditedTime { get; init; }
    public bool Archived { get; init; }
    
    public User CreatedBy { get; init; }
    public User LastEditedBy { get; init; }

    public string Url { get; init; }
    public string PublicUrl { get; init; }

    /// <summary>
    /// TODO: polymorph converter (for type=database)
    /// </summary>
    [JsonConverter(typeof(PageParentJsonConverter))]
    public PageParent Parent { get; init; }


    /// <remarks>
    /// Removed, instead use https://developers.notion.com/reference/retrieve-a-page-property 
    /// If parent.type is "page_id" or "workspace", then the only valid key is "title".
    /// If parent.type is "database_id", then the keys and values of this field are determined by the properties of the database this page belongs to.
    /// </remarks>
    public Dictionary<string, PropertyItem>? Properties { get; init; }

    // [JsonIgnore]
    // public PropertyTitle? Title => Properties != null ? Properties.TryGetValue("title", out var title) ? title.ToObject<PropertyTitle>(HttpNotionSession.NotionJsonSerializationOptions) : default : default;
            
    #region Parent polymorphism
    //TODO: replace with integrated polymorphism
    public abstract class PageParent
    {
        public string Type { get; init; }

        public const string TypeWorkspace = "workspace";
        public const string TypePage = "page_id";
        public const string TypeDatabase = "database_id";
    }

    public class PageParentWorkspace : PageParent {}

    public class PageParentPage : PageParent
    {
        [JsonPropertyName("page_id")]
        public string PageId { get; init; }
    }

    public class PageParentDatabase : PageParent
    {
        [JsonPropertyName("database_id")]
        public string DatabaseId { get; init; }
    }
            
    public class PageParentJsonConverter : JsonConverter<PageParent>
    {
        private static readonly Dictionary<string, Type> PolymorphismTypes = new()
        {
            { "workspace", typeof(PageParentWorkspace) },
            { "page_id", typeof(PageParentPage) },
            { "database_id", typeof(PageParentDatabase) },
        };
            
        public override bool CanConvert(Type typeToConvert) => typeof(PageParent).IsAssignableFrom(typeToConvert);

        public override PageParent? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var node = JsonNode.Parse(ref reader) as JsonObject;
            if (node?.TryGetPropertyValue("type", out var oType) == true)
            {
                var type = oType?.GetValue<string>();
                if (type == null || !PolymorphismTypes.TryGetValue(type, out var parentType))
                    throw new JsonException($"Error deserializing PageParent: unknown type '{type ?? "null"}'");
                return (PageParent?)node.Deserialize(parentType, options);
            }

            return null;
        }

        public override void Write(Utf8JsonWriter writer, PageParent value, JsonSerializerOptions options)
            => JsonSerializer.Serialize(writer, value, options);
    }
    #endregion

    //TODO: polymorph converter (for type=database)
    //See https://developers.notion.com/reference/page#page-property-value
    // public abstract class PagePropertyValue : BaseObject
    // {
    //     /// <summary>
    //     /// Possible values are:
    //     /// "rich_text", "number", "select", "multi_select", "date", "formula", "relation", "rollup", "title", "people", "files", "checkbox", "url", "email", "phone_number", "created_time", "created_by", "last_edited_time", "last_edited_by".
    //     /// </summary>
    //     public string Type { get; set; }
    // }
            
}