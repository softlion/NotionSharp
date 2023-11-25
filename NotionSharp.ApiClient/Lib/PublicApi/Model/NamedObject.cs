using System;
using System.Text.Json.Serialization;

namespace NotionSharp.ApiClient;

public abstract class ApiObject
{
    [JsonIgnore]
    public string? JsonOriginal { get; set; }
    [JsonIgnore]
    public Exception? Exception { get; set; }
}
    
[JsonPolymorphic(TypeDiscriminatorPropertyName = "object",
    UnknownDerivedTypeHandling = JsonUnknownDerivedTypeHandling.FallBackToBaseType,
    IgnoreUnrecognizedTypeDiscriminators = true
)]
[JsonDerivedType(typeof(Block), "block")]
[JsonDerivedType(typeof(Page), "page")]
[JsonDerivedType(typeof(PropertyItem), "property_item")]
public class NamedObject : ApiObject
{
    public string Object { get; init; } = null!;
}