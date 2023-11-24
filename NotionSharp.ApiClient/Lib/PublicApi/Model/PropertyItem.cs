using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace NotionSharp.ApiClient;

public class NestedPolymorphicJsonConverter : JsonConverter<PropertyItem>
{
    public override PropertyItem Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartObject)
            throw new JsonException();

        using var document = JsonDocument.ParseValue(ref reader);
        var type = document.RootElement.GetProperty("type").GetString() 
                   ?? throw new JsonException("Missing property 'type'");

        var id = document.RootElement.GetProperty("id").GetString()
                 ?? throw new JsonException("Missing property 'id'");

        switch (type)
        {
            case "rich_text":
                var richText = JsonSerializer.Deserialize<RichText>(document.RootElement.GetProperty("rich_text").GetRawText(), options) 
                               ?? throw new JsonException("Failed to deserialize 'rich_text'");
                return new RichTextPropertyItem { Id = id, Type = type, RichText = richText };
            case "number":
                var number = document.RootElement.GetProperty("number").GetInt32();
                return new NumberPropertyItem { Id = id, Type = type, Number = number };
            case "title":
                var title = JsonSerializer.Deserialize<PropertyTitle>(document.RootElement.GetProperty("title").GetRawText(), options)
                            ?? throw new JsonException("Failed to deserialize 'title'");
                return new TitlePropertyItem { Id = id, Type = type, Title = title };
            default:
                //Type is not implemented, but still return the base type
                return new() { Id = id, Type = type };
        }
    }

    public override void Write(Utf8JsonWriter writer, PropertyItem value, JsonSerializerOptions options) 
        => throw new NotImplementedException();
}

/// <summary>
/// https://developers.notion.com/reference/property-item-object
/// </summary>
/// <remarks>
/// JsonPolymorphic is not working because the base type is already a JsonPolymorphic,
/// and is not using the same discriminator.
/// </remarks>
[JsonConverter(typeof(NestedPolymorphicJsonConverter))]
// [JsonPolymorphic(TypeDiscriminatorPropertyName = nameof(Type),
//     UnknownDerivedTypeHandling = JsonUnknownDerivedTypeHandling.FallBackToNearestAncestor,
//     IgnoreUnrecognizedTypeDiscriminators = true
// )] 
// [JsonDerivedType(typeof(RichTextPropertyItem), "rich_text")]
// [JsonDerivedType(typeof(NumberPropertyItem), "number")]
// [JsonDerivedType(typeof(TitlePropertyItem), "title")]
#region TODO, currently fallback to PropertyItem
// [JsonDerivedType(typeof(PropertyItem), "select")]
// [JsonDerivedType(typeof(PropertyItem), "multi_select")]
// [JsonDerivedType(typeof(PropertyItem), "date")]
// [JsonDerivedType(typeof(PropertyItem), "formula")]
// [JsonDerivedType(typeof(PropertyItem), "relation")]
// [JsonDerivedType(typeof(PropertyItem), "rollup")]
// [JsonDerivedType(typeof(PropertyItem), "people")]
// [JsonDerivedType(typeof(PropertyItem), "files")]
// [JsonDerivedType(typeof(PropertyItem), "checkbox")]
// [JsonDerivedType(typeof(PropertyItem), "url")]
// [JsonDerivedType(typeof(PropertyItem), "email")]
// [JsonDerivedType(typeof(PropertyItem), "phone_number")]
// [JsonDerivedType(typeof(PropertyItem), "created_time")]
// [JsonDerivedType(typeof(PropertyItem), "created_by")]
// [JsonDerivedType(typeof(PropertyItem), "last_edited_time")]
// [JsonDerivedType(typeof(PropertyItem), "last_edited_by")]
#endregion
public class PropertyItem : NamedObject
{
    public string Id { get; set; }
    /// <summary>
    /// "rich_text", "number", "title", 
    /// TODO: "select", "multi_select", "date", "formula", "relation", "rollup", "people", "files", "checkbox", "url", "email", "phone_number", "created_time", "created_by", "last_edited_time", and "last_edited_by"
    /// </summary>
    public string Type { get; set; }
        
    //public Relation? Relation { get; set; }
    //public People? People { get; set; }
}

public class RichTextPropertyItem : PropertyItem
{
    public RichText RichText { get; init; }
}
public class NumberPropertyItem : PropertyItem
{
    public int Number { get; init; }
}
public class TitlePropertyItem : PropertyItem
{
    public PropertyTitle Title { get; init; }
}