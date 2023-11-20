using System.Text.Json.Serialization;

namespace NotionSharp.ApiClient;

/// <summary>
/// https://developers.notion.com/reference/property-item-object
/// </summary>
[JsonPolymorphic(TypeDiscriminatorPropertyName = nameof(Type),
    UnknownDerivedTypeHandling = JsonUnknownDerivedTypeHandling.FallBackToBaseType,
    IgnoreUnrecognizedTypeDiscriminators = true
)] 
[JsonDerivedType(typeof(RichTextPropertyItem), "rich_text")]
[JsonDerivedType(typeof(NumberPropertyItem), "number")]
[JsonDerivedType(typeof(TitlePropertyItem), "title")]
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