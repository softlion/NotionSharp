using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using NotionSharp.ApiClient.Lib;

namespace NotionSharp.ApiClient
{
    public class Page : BaseObject
    {
        public string Id { get; set; }
            
        [JsonPropertyName("created_time")]
        public DateTimeOffset CreatedTime { get; set; }
        [JsonPropertyName("last_edited_time")]
        public DateTimeOffset LastEditedTime { get; set; }
        public bool Archived { get; set; }
        
        [JsonConverter(typeof(PageParentJsonConverter))]
        public PageParent Parent { get; set; }
            
        /// <summary>
        /// If parent.type is "page_id" or "workspace", then the only valid key is "title".
        /// If parent.type is "database_id", then the keys and values of this field are determined by the properties of the database this page belongs to.
        /// </summary>
        public Dictionary<string, JsonElement>? Properties { get; set; }
        //TODO: polymorph converter (for type=database)
        //public Dictionary<string, PagePropertyValue>? Properties { get; set; }

        [JsonIgnore]
        public PropertyTitle? Title => Properties != null ? Properties.TryGetValue("title", out var title) ? title.ToObject<PropertyTitle>(HttpNotionSession.NotionJsonSerializationOptions) : default : default;
            
        #region Parent polymorphism
        public abstract class PageParent
        {
            /// <summary>
            /// database_id, page_id, workspace
            /// </summary>
            public string Type { get; set; }
        }

        public class PageParentWorkspace : PageParent {}

        public class PageParentPage : PageParent
        {
            [JsonPropertyName("page_id")]
            public string PageId { get; set; }
        }

        public class PageParentDatabase : PageParent
        {
            [JsonPropertyName("database_id")]
            public string DatabaseId { get; set; }
        }
            
        public class PageParentJsonConverter : JsonConverter<PageParent>
        {
            private static readonly Dictionary<string, Type> PolymorphismTypes = new Dictionary<string, Type>
            {
                { "workspace", typeof(PageParentWorkspace) },
                { "page_id", typeof(PageParentPage) },
                { "database_id", typeof(PageParentDatabase) },
            };
            
            public override bool CanConvert(Type typeToConvert) => typeof(PageParent).IsAssignableFrom(typeToConvert);

            public override PageParent Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            {
                var element = JsonSerializer.Deserialize<JsonElement>(ref reader);
                var type = element.EnumerateObject().FirstOrDefault(p => p.Name == "type").Value.GetString();
                if(type == null || !PolymorphismTypes.TryGetValue(type, out var parentType))
                    throw new JsonException($"Error deserializing PageParent: unknown type '{type ?? "null"}'");
                return (PageParent)element.ToObject(parentType, options);
            }

            public override void Write(Utf8JsonWriter writer, PageParent value, JsonSerializerOptions options)
                => JsonSerializer.Serialize(writer, value, options);
        }
        #endregion

        public class PropertyTitle
        {
            public string Type { get; set; } = "title";
            public string Id { get; set; }
            public List<RichText> Title { get; set; }
        }

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
}